using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Basically.Editor.Weaver {
    using Serialization;

    internal class WriterProcessor {
        Dictionary<TypeReference, MethodReference> allWrites;
        Dictionary<TypeReference, MethodReference> allBitWrites;
        Dictionary<TypeReference, MethodReference> allDeltaWrites;

        Dictionary<TypeReference, MethodReference> currentWrites;
        Dictionary<TypeReference, MethodReference> currentBitWrites;
        Dictionary<TypeReference, MethodReference> currentDeltaWrites;

        ModuleDefinition mainModule;
        TypeDefinition genClass;

        private WriterProcessor() {
            allWrites = CreateDictionary();
            allBitWrites = CreateDictionary();
            currentBitWrites = CreateDictionary();
            currentWrites = CreateDictionary();
        }

        public WriterProcessor(ModuleDefinition mod, TypeDefinition gen) : this() {
            Reset(mod, gen);
        }

        private Dictionary<TypeReference, MethodReference> CreateDictionary() {
            return new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        }

        public void Reset(ModuleDefinition mod, TypeDefinition gen) {
            mainModule = mod;
            genClass = gen;
            currentWrites.Clear();
            currentBitWrites.Clear();

            if (allWrites.Count != 0) { // import regular
                var imported = CreateDictionary();
                foreach (var pair in allWrites) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                allWrites = imported;
            }

            if (allBitWrites.Count != 0) { // import bit
                var imported = CreateDictionary();
                foreach (var pair in allBitWrites) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                allBitWrites = imported;
            }
        }

        public void Register(TypeReference dataType, MethodReference method, bool bit = false) {
            if (allWrites.ContainsKey(dataType)) {
                return;
            }

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            TypeReference imported = mainModule.ImportReference(dataType);

            if (bit) {
                allBitWrites[imported] = method;
                currentBitWrites[imported] = method;

                GenerateBitToRegular(imported, method); // creates a method that calls the bit version from regular calls
            } else {
                allWrites[imported] = method;
                currentWrites[imported] = method;
            }
        }

        void RegisterWriteFunc(TypeReference typeReference, MethodDefinition newWriterFunc) {
            Register(typeReference, newWriterFunc);

            genClass.Methods.Add(newWriterFunc);
        }

        /// <summary>
        /// Finds existing writer for type, if non exists trys to create one
        /// <para>This method is recursive</para>
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>Returns <see cref="MethodReference"/> or null</returns>
        public MethodReference GetWriteFunc(TypeReference variable, bool bit = false) {
            // only make bitless writers
            if (!bit) {
                if (allWrites.TryGetValue(variable, out MethodReference foundFunc)) {
                    return foundFunc;
                } else {
                    // this try/catch will be removed in future PR and make `GetWriteFunc` throw instead
                    try {
                        TypeReference importedVariable = mainModule.ImportReference(variable);
                        return GenerateWriter(importedVariable);
                    } catch (ArgumentException e) {
                        // Weaver.Error(e.Message, e.MemberReference);
                        return null;
                    }
                }
            } else {
                if (allBitWrites.TryGetValue(variable, out var found)) {
                    return found;
                } else {
                    return null;
                }
            }
            
        }

        /// <exception cref="ArgumentException">Throws when writer could not be generated for type</exception>
        MethodReference GenerateWriter(TypeReference variableReference) {
            if (variableReference.IsByReference) {
                throw new ArgumentException($"Cannot pass {variableReference.Name} by reference");
            }

            // Arrays are special, if we resolve them, we get the element type,
            // e.g. int[] resolves to int
            // therefore process this before checks below
            if (variableReference.IsArray) {
                if (variableReference.IsMultidimensionalArray()) {
                    throw new ArgumentException($"{variableReference.Name} is an unsupported type. Multidimensional arrays are not supported");
                }
                TypeReference elementType = variableReference.GetElementType();
                return GenerateCollectionWriter(variableReference, elementType, nameof(WriterExtensions.WriteArray));
            }

            if (variableReference.Resolve()?.IsEnum ?? false) {
                // serialize enum as their base type
                return GenerateEnumWriteFunc(variableReference);
            }

            // replace with entity
            // if (variableReference.Inherits<NetworkBehaviour>()) {
            //     return GetNetworkBehaviourWriter(variableReference);
            // }

            // check for invalid types
            TypeDefinition variableDefinition = variableReference.Resolve();
            if (variableDefinition == null) {
                throw new ArgumentException($"{variableReference.Name} is not a supported type. Use a supported type or provide a custom writer");
            }
            if (variableDefinition.Inherits<UnityEngine.Component>()) {
                throw new ArgumentException($"Cannot generate writer for component type {variableReference.Name}. Use a supported type or provide a custom writer");
            }
            if (variableReference.Is<UnityEngine.Object>()) {
                throw new ArgumentException($"Cannot generate writer for {variableReference.Name}. Use a supported type or provide a custom writer");
            }
            if (variableReference.Is<UnityEngine.ScriptableObject>()) {
                throw new ArgumentException($"Cannot generate writer for {variableReference.Name}. Use a supported type or provide a custom writer");
            }
            if (variableDefinition.HasGenericParameters) {
                throw new ArgumentException($"Cannot generate writer for generic type {variableReference.Name}. Use a supported type or provide a custom writer");
            }
            if (variableDefinition.IsInterface) {
                throw new ArgumentException($"Cannot generate writer for interface {variableReference.Name}. Use a supported type or provide a custom writer");
            }
            if (variableDefinition.IsAbstract) {
                throw new ArgumentException($"Cannot generate writer for abstract class {variableReference.Name}. Use a supported type or provide a custom writer");
            }

            // generate writer for class/struct
            return GenerateClassOrStructWriterFunction(variableReference);
        }

        MethodDefinition GenerateEnumWriteFunc(TypeReference variable) {
            MethodDefinition writerFunc = GenerateWriterFunc(variable);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            MethodReference underlyingWriter = GetWriteFunc(variable.Resolve().GetEnumUnderlyingType());

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, underlyingWriter);

            worker.Emit(OpCodes.Ret);
            return writerFunc;
        }

        MethodDefinition GenerateWriterFunc(TypeReference variable) {
            string functionName = "_Write_" + variable.FullName;
            // create new writer for this type
            MethodDefinition writerFunc = new MethodDefinition(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig,
                    mainModule.ImportReference(typeof(void)));

            writerFunc.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, mainModule.ImportReference(typeof(Writer))));
            writerFunc.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, variable));
            writerFunc.Body.InitLocals = true;

            RegisterWriteFunc(variable, writerFunc);
            return writerFunc;
        }

        MethodDefinition GenerateClassOrStructWriterFunction(TypeReference variable) {
            MethodDefinition writerFunc = GenerateWriterFunc(variable);

            ILProcessor worker = writerFunc.Body.GetILProcessor();

            if (!variable.Resolve().IsValueType)
                WriteNullCheck(worker);

            if (!WriteAllFields(variable, worker))
                return null;

            worker.Emit(OpCodes.Ret);
            return writerFunc;
        }

        void WriteNullCheck(ILProcessor worker) {
            // if (value == null)
            // {
            //     writer.WriteBoolean(false);
            //     return;
            // }
            //

            Instruction labelNotNull = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Brtrue, labelNotNull);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldc_I4_0);
            worker.Emit(OpCodes.Call, GetWriteFunc(mainModule.ImportReference(typeof(bool))));
            worker.Emit(OpCodes.Ret);
            worker.Append(labelNotNull);

            // write.WriteBoolean(true);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldc_I4_1);
            worker.Emit(OpCodes.Call, GetWriteFunc(mainModule.ImportReference(typeof(bool))));
        }

        /// <summary>
        /// Find all fields in type and write them
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="worker"></param>
        /// <returns>false if fail</returns>
        bool WriteAllFields(TypeReference variable, ILProcessor worker) {
            uint fields = 0;
            foreach (FieldDefinition field in variable.FindAllPublicFields()) {
                bool isBitWrite = field.HasCustomAttribute<BitSizeAttribute>(out var attr);
                MethodReference writeFunc = GetWriteFunc(field.FieldType, isBitWrite);
                if (writeFunc == null) { return false; }

                FieldReference fieldRef = mainModule.ImportReference(field);

                fields++;
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldfld, fieldRef);
                if (isBitWrite) worker.Emit(OpCodes.Ldc_I4, (int)attr.ConstructorArguments[0].Value);
                worker.Emit(OpCodes.Call, writeFunc);
            }

            return true;
        }

        MethodDefinition GenerateCollectionWriter(TypeReference variable, TypeReference elementType, string writerFunction) {

            MethodDefinition writerFunc = GenerateWriterFunc(variable);

            MethodReference elementWriteFunc = GetWriteFunc(elementType);
            MethodReference intWriterFunc = GetWriteFunc(mainModule.ImportReference(typeof(int)));

            // need this null check till later PR when GetWriteFunc throws exception instead
            if (elementWriteFunc == null) {
                // Weaver.Error($"Cannot generate writer for {variable}. Use a supported type or provide a custom writer", variable);
                return writerFunc;
            }

            TypeReference extend = mainModule.ImportReference(typeof(WriterExtensions));
            TypeDefinition resolved = extend.SafeResolve(mainModule);
            MethodReference collectionWriter = mainModule.ImportReference(resolved.Methods.First(x => x.Name == writerFunction));

            GenericInstanceMethod methodRef = new GenericInstanceMethod(collectionWriter);
            methodRef.GenericArguments.Add(elementType);

            // generates
            // reader.WriteArray<T>(array);

            ILProcessor worker = writerFunc.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0); // writer
            worker.Emit(OpCodes.Ldarg_1); // collection

            worker.Emit(OpCodes.Call, methodRef); // WriteArray

            worker.Emit(OpCodes.Ret);

            return writerFunc;
        }

        MethodDefinition GenerateBitToRegular(TypeReference tr, MethodReference methodRef) {
            var method = GenerateWriterFunc(tr);
            var worker = method.Body.GetILProcessor();

            worker.Emit(OpCodes.Ldarg_0); // load writer
            worker.Emit(OpCodes.Ldarg_1); // load value
            worker.Emit(OpCodes.Ldc_I4, (int)methodRef.Parameters[2].Constant);
            worker.Emit(OpCodes.Call, methodRef);
            worker.Emit(OpCodes.Nop);
            worker.Emit(OpCodes.Ret);

            return method;
        }

        MethodDefinition GenerateEntityFunc(TypeReference tr) {
            var method = GenerateWriterFunc(tr);



            return method;
        }

        /// <summary>
        /// Save a delegate for each one of the writers into <see cref="TypeData{T}.write"/>
        /// </summary>
        /// <param name="worker"></param>
        internal void InitializeWriters(ILProcessor worker) {
            TypeReference genericWriterClassRef = mainModule.ImportReference(typeof(TypeData<>));
            TypeReference writerRef = mainModule.ImportReference(typeof(Writer));

            { // bit pass
                System.Reflection.FieldInfo fieldInfo = typeof(TypeData<>).GetField(nameof(TypeData<object>.writeBit));
                FieldReference fieldRef = mainModule.ImportReference(fieldInfo);
                TypeReference actionRef = mainModule.ImportReference(typeof(Action<,,>));
                TypeReference intRef = mainModule.ImportReference(typeof(int));
                MethodReference actionConstructorRef = mainModule.ImportReference(typeof(Action<,,>).GetConstructors()[0]);
            
                foreach (KeyValuePair<TypeReference, MethodReference> kvp in currentBitWrites) {
                    TypeReference targetType = kvp.Key;
                    MethodReference writeFunc = kvp.Value;
            
                    // create a Action<NetworkWriter, T, int> delegate
                    worker.Emit(OpCodes.Ldnull);
                    worker.Emit(OpCodes.Ldftn, writeFunc);
                    GenericInstanceType actionGenericInstance = actionRef.MakeGenericInstanceType(writerRef, targetType, intRef);
                    MethodReference actionRefInstance = actionConstructorRef.MakeHostInstanceGeneric(actionGenericInstance, mainModule);
                    worker.Emit(OpCodes.Newobj, actionRefInstance);
            
                    // save it in Writer<T>.writeBit
                    GenericInstanceType genericInstance = genericWriterClassRef.MakeGenericInstanceType(targetType);
                    FieldReference specializedField = fieldRef.SpecializeField(genericInstance);
                    worker.Emit(OpCodes.Stsfld, specializedField);
                }
            }

            { // regular pass
                System.Reflection.FieldInfo fieldInfo = typeof(TypeData<>).GetField(nameof(TypeData<object>.write));
                FieldReference fieldRef = mainModule.ImportReference(fieldInfo);
                TypeReference actionRef = mainModule.ImportReference(typeof(Action<,>));
                MethodReference actionConstructorRef = mainModule.ImportReference(typeof(Action<,>).GetConstructors()[0]);

                foreach (KeyValuePair<TypeReference, MethodReference> kvp in currentWrites) {
                    TypeReference targetType = kvp.Key;
                    MethodReference writeFunc = kvp.Value;

                    // create a Action<NetworkWriter, T> delegate
                    worker.Emit(OpCodes.Ldnull);
                    worker.Emit(OpCodes.Ldftn, writeFunc);
                    GenericInstanceType actionGenericInstance = actionRef.MakeGenericInstanceType(writerRef, targetType);
                    MethodReference actionRefInstance = actionConstructorRef.MakeHostInstanceGeneric(actionGenericInstance, mainModule);
                    worker.Emit(OpCodes.Newobj, actionRefInstance);

                    // save it in Writer<T>.write
                    GenericInstanceType genericInstance = genericWriterClassRef.MakeGenericInstanceType(targetType);
                    FieldReference specializedField = fieldRef.SpecializeField(genericInstance);
                    worker.Emit(OpCodes.Stsfld, specializedField);
                }
            }
        }
    }
}
