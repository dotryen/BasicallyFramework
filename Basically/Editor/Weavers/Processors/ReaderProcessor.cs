using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using riles.Weaver;

namespace Basically.Editor {
    using Serialization;

    public class ReaderProcessor {
        Dictionary<TypeReference, MethodReference> allReads;
        Dictionary<TypeReference, MethodReference> allBitReads;
        Dictionary<TypeReference, MethodReference> currentReads;
        Dictionary<TypeReference, MethodReference> currentBitReads;

        ModuleDefinition mainModule;
        TypeDefinition genClass;

        private ReaderProcessor() {
            allReads = CreateDictionary();
            allBitReads = CreateDictionary();
            currentBitReads = CreateDictionary();
            currentReads = CreateDictionary();
        }

        public ReaderProcessor(ModuleDefinition mod, TypeDefinition gen) : this() {
            Reset(mod, gen);
        }

        private Dictionary<TypeReference, MethodReference> CreateDictionary() {
            return new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        }

        public void Reset(ModuleDefinition mod, TypeDefinition gen) {
            mainModule = mod;
            genClass = gen;
            currentReads.Clear();
            currentBitReads.Clear();

            if (allReads.Count != 0) { // import regular
                var imported = CreateDictionary();
                foreach (var pair in allReads) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                allReads = imported;
            }

            if (allBitReads.Count != 0) { // import bit
                var imported = CreateDictionary();
                foreach (var pair in allBitReads) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                allBitReads = imported;
            }
        }

        internal void Register(TypeReference dataType, MethodReference method, bool bit = false) {
            if (allReads.ContainsKey(dataType)) {
                return;
            }

            // we need to import type when we Initialize Readers so import here in case it is used anywhere else
            TypeReference imported = mainModule.ImportReference(dataType);

            if (bit) {
                allBitReads[imported] = method;
                currentBitReads[imported] = method;

                GenerateBitToRegular(imported, method);
            } else {
                allReads[imported] = method;
                currentReads[imported] = method;
            }
        }

        void RegisterReadFunc(TypeReference typeReference, MethodDefinition newReaderFunc) {
            Register(typeReference, newReaderFunc);

            genClass.Methods.Add(newReaderFunc);
        }

        /// <summary>
        /// Finds existing reader for type, if non exists trys to create one
        /// <para>This method is recursive</para>
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>Returns <see cref="MethodReference"/> or null</returns>
        public MethodReference GetReadFunc(TypeReference variable, bool bit = false) {
            // only generate regular reads

            if (!bit) {
                if (allReads.TryGetValue(variable, out MethodReference foundFunc)) {
                    return foundFunc;
                } else {
                    TypeReference importedVariable = mainModule.ImportReference(variable);
                    return GenerateReader(importedVariable);
                }
            } else {
                if (allBitReads.TryGetValue(variable, out var found)) {
                    return found;
                } else {
                    return null;
                }
            }
        }

        MethodReference GenerateReader(TypeReference variableReference) {
            // Arrays are special,  if we resolve them, we get the element type,
            // so the following ifs might choke on it for scriptable objects
            // or other objects that require a custom serializer
            // thus check if it is an array and skip all the checks.
            if (variableReference.IsArray) {
                if (variableReference.IsMultidimensionalArray()) {
                    // Weaver.Error($"{variableReference.Name} is an unsupported type. Multidimensional arrays are not supported", variableReference);
                    return null;
                }
            
                return GenerateReadCollection(variableReference, variableReference.GetElementType(), nameof(ReaderExtensions.ReadArray));
            }
            
            TypeDefinition variableDefinition = variableReference.Resolve();
            if (variableDefinition == null) {
                // Weaver.Error($"{variableReference.Name} is not a supported type", variableReference);
                return null;
            }
            // if (variableDefinition.Inherits<UnityEngine.Component>() &&
            //     !variableReference.Inherits<NetworkBehaviour>()) {
            //     // Weaver.Error($"Cannot generate reader for component type {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
            //     return null;
            // }
            if (variableReference.Is<UnityEngine.Object>()) {
                // Weaver.Error($"Cannot generate reader for {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
                return null;
            }
            if (variableReference.Is<UnityEngine.ScriptableObject>()) {
                // Weaver.Error($"Cannot generate reader for {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
                return null;
            }
            if (variableReference.IsByReference) {
                // error??
                // Weaver.Error($"Cannot pass type {variableReference.Name} by reference", variableReference);
                return null;
            }
            if (variableDefinition.HasGenericParameters) {
                // Weaver.Error($"Cannot generate reader for generic variable {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
                return null;
            }
            if (variableDefinition.IsInterface) {
                // Weaver.Error($"Cannot generate reader for interface {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
                return null;
            }
            if (variableDefinition.IsAbstract) {
                // Weaver.Error($"Cannot generate reader for abstract class {variableReference.Name}. Use a supported type or provide a custom reader", variableReference);
                return null;
            }
             
            if (variableDefinition.IsEnum) {
                return GenerateEnumReadFunc(variableReference);
            }
            
            return GenerateClassOrStructReadFunction(variableReference);
        }

        MethodDefinition GenerateEnumReadFunc(TypeReference variable) {
            MethodDefinition readerFunc = GenerateReaderFunction(variable);
            
            ILProcessor worker = readerFunc.Body.GetILProcessor();
            
            worker.Emit(OpCodes.Ldarg_0);
            
            TypeReference underlyingType = variable.Resolve().GetEnumUnderlyingType();
            MethodReference underlyingFunc = GetReadFunc(underlyingType);
            
            worker.Emit(OpCodes.Call, underlyingFunc);
            worker.Emit(OpCodes.Ret);
            return readerFunc;
        }

        MethodDefinition GenerateReaderFunction(TypeReference variable) {
            string functionName = "_Read_" + variable.FullName;
            
            // create new reader for this type
            MethodDefinition readerFunc = new MethodDefinition(functionName,
                    MethodAttributes.Public |
                    MethodAttributes.Static |
                    MethodAttributes.HideBySig,
                    variable);
            
            readerFunc.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, mainModule.ImportReference(typeof(Reader))));
            readerFunc.Body.InitLocals = true;
            RegisterReadFunc(variable, readerFunc);
            
            return readerFunc;
        }

        MethodDefinition GenerateReadCollection(TypeReference variable, TypeReference elementType, string readerFunction) {
            MethodDefinition readerFunc = GenerateReaderFunction(variable);
            // generate readers for the element
            GetReadFunc(elementType);
            
            TypeReference extend = mainModule.ImportReference(typeof(ReaderExtensions));
            TypeDefinition resolved = extend.SafeResolve(mainModule);
            MethodReference listReader = mainModule.ImportReference(resolved.Methods.First(x => x.Name == readerFunction));
            
            GenericInstanceMethod methodRef = new GenericInstanceMethod(listReader);
            methodRef.GenericArguments.Add(elementType);
            
            // generates
            // return reader.ReadList<T>();
            
            ILProcessor worker = readerFunc.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0); // reader
            worker.Emit(OpCodes.Call, methodRef); // Read
            
            worker.Emit(OpCodes.Ret);
            
            return readerFunc;
        }

        MethodDefinition GenerateClassOrStructReadFunction(TypeReference variable) {
            MethodDefinition readerFunc = GenerateReaderFunction(variable);
             
            // create local for return value
            readerFunc.Body.Variables.Add(new VariableDefinition(variable));
            
            ILProcessor worker = readerFunc.Body.GetILProcessor();
            
            TypeDefinition td = variable.Resolve();
            
            if (!td.IsValueType)
                GenerateNullCheck(worker);
            
            CreateNew(variable, worker, td);
            ReadAllFields(variable, worker);
            
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ret);
            return readerFunc;
        }

        void GenerateNullCheck(ILProcessor worker) {
            // if (!reader.ReadBoolean()) {
            //   return null;
            // }
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, GetReadFunc(mainModule.ImportReference(typeof(bool))));

            Instruction labelEmptyArray = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Brtrue, labelEmptyArray);
            // return null
            worker.Emit(OpCodes.Ldnull);
            worker.Emit(OpCodes.Ret);
            worker.Append(labelEmptyArray);
        }

        // Initialize the local variable with a new instance
         void CreateNew(TypeReference variable, ILProcessor worker, TypeDefinition td) {
            if (variable.IsValueType) {
                // structs are created with Initobj
                worker.Emit(OpCodes.Ldloca, 0);
                worker.Emit(OpCodes.Initobj, variable);
            } else {
                // classes are created with their constructor
                MethodDefinition ctor = variable.Resolve().Methods.First(x => x.Name == ".ctor" && x.Resolve().IsPublic && x.Parameters.Count == 0);
                if (ctor == null) {
                    // Weaver.Error($"{variable.Name} can't be deserialized because it has no default constructor", variable);
                    return;
                }

                MethodReference ctorRef = mainModule.ImportReference(ctor);

                worker.Emit(OpCodes.Newobj, ctorRef);
                worker.Emit(OpCodes.Stloc_0);
            }
        }

        void ReadAllFields(TypeReference variable, ILProcessor worker) {
            foreach (FieldDefinition field in variable.FindAllPublicFields()) {

                OpCode opcode = variable.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc;
                worker.Emit(opcode, 0);

                var isBit = field.HasCustomAttribute<BitSizeAttribute>(out var attr);
                MethodReference readFunc = GetReadFunc(field.FieldType, isBit);

                if (readFunc != null) {
                    worker.Emit(OpCodes.Ldarg_0);
                    if (isBit) worker.Emit(OpCodes.Ldc_I4, (int)attr.ConstructorArguments[0].Value);
                    worker.Emit(OpCodes.Call, readFunc);
                } else {
                    // Weaver.Error($"{field.Name} has an unsupported type", field);
                }
                FieldReference fieldRef = mainModule.ImportReference(field);

                worker.Emit(OpCodes.Stfld, fieldRef);
            }
        }

        MethodDefinition GenerateBitToRegular(TypeReference tr, MethodReference methodRef) {
            var method = GenerateReaderFunction(tr);
            var worker = method.Body.GetILProcessor();

            worker.Emit(OpCodes.Ldarg_0); // load reader
            worker.Emit(OpCodes.Ldc_I4, (int)methodRef.Parameters[1].Constant); // set bit constant
            worker.Emit(OpCodes.Call, methodRef);
            worker.Emit(OpCodes.Nop);
            worker.Emit(OpCodes.Ret);

            return method;
        }

        /// <summary>
        /// Save a delegate for each one of the readers into <see cref="Reader{T}.read"/>
        /// </summary>
        /// <param name="worker"></param>
        internal void InitializeReaders(ILProcessor worker) {
            TypeReference genericReaderClassRef = mainModule.ImportReference(typeof(TypeData<>));

            { // bit pass
                System.Reflection.FieldInfo fieldInfo = typeof(TypeData<>).GetField(nameof(TypeData<object>.readBit));
                FieldReference fieldRef = mainModule.ImportReference(fieldInfo);
                TypeReference readerRef = mainModule.ImportReference(typeof(Reader));
                TypeReference intRef = mainModule.ImportReference(typeof(int));
                TypeReference funcRef = mainModule.ImportReference(typeof(Func<,,>));
                MethodReference funcConstructorRef = mainModule.ImportReference(typeof(Func<,,>).GetConstructors()[0]);

                foreach (KeyValuePair<TypeReference, MethodReference> kvp in currentBitReads) {
                    TypeReference targetType = kvp.Key;
                    MethodReference readFunc = kvp.Value;

                    // create a Func<NetworkReader, int, T> delegate
                    worker.Emit(OpCodes.Ldnull);
                    worker.Emit(OpCodes.Ldftn, readFunc);
                    GenericInstanceType funcGenericInstance = funcRef.MakeGenericInstanceType(readerRef, intRef, targetType);
                    MethodReference funcConstructorInstance = funcConstructorRef.MakeHostInstanceGeneric(funcGenericInstance, mainModule);
                    worker.Emit(OpCodes.Newobj, funcConstructorInstance);

                    // save it in Reader<T>.readBit
                    GenericInstanceType genericInstance = genericReaderClassRef.MakeGenericInstanceType(targetType);
                    FieldReference specializedField = fieldRef.SpecializeField(genericInstance);
                    worker.Emit(OpCodes.Stsfld, specializedField);
                }
            }

            { // regular pass
                System.Reflection.FieldInfo fieldInfo = typeof(TypeData<>).GetField(nameof(TypeData<object>.read));
                FieldReference fieldRef = mainModule.ImportReference(fieldInfo);
                TypeReference networkReaderRef = mainModule.ImportReference(typeof(Reader));
                TypeReference funcRef = mainModule.ImportReference(typeof(Func<,>));
                MethodReference funcConstructorRef = mainModule.ImportReference(typeof(Func<,>).GetConstructors()[0]);

                foreach (KeyValuePair<TypeReference, MethodReference> kvp in currentReads) {
                    TypeReference targetType = kvp.Key;
                    MethodReference readFunc = kvp.Value;

                    // create a Func<NetworkReader, T> delegate
                    worker.Emit(OpCodes.Ldnull);
                    worker.Emit(OpCodes.Ldftn, readFunc);
                    GenericInstanceType funcGenericInstance = funcRef.MakeGenericInstanceType(networkReaderRef, targetType);
                    MethodReference funcConstructorInstance = funcConstructorRef.MakeHostInstanceGeneric(funcGenericInstance, mainModule);
                    worker.Emit(OpCodes.Newobj, funcConstructorInstance);

                    // save it in Reader<T>.read
                    GenericInstanceType genericInstance = genericReaderClassRef.MakeGenericInstanceType(targetType);
                    FieldReference specializedField = fieldRef.SpecializeField(genericInstance);
                    worker.Emit(OpCodes.Stsfld, specializedField);
                }
            }
        }
    }
}
