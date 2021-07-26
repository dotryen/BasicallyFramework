using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UnityEngine;

namespace Basically.Editor.Weaver {
    using Serialization;
    using Networking;

    internal class ReaderWriterWeaver : Weaver {
        public override int Priority => 0;
        public override bool IsEditor => false;

        public List<MethodDefinition> writers = new List<MethodDefinition>();
        public List<MethodDefinition> readers = new List<MethodDefinition>();

        Dictionary<TypeReference, MethodReference> writerCache = new Dictionary<TypeReference, MethodReference>();

        public override void Weave() {
            foreach (var type in Module.Types.Where(x => x.IsSealed && x.IsAbstract)) {
                GetAllWriters(type);
                GetAllReaders(type);
            }

            foreach (var type in Module.Types.Where(x => !x.IsAbstract && !x.IsInterface && x.Implements<NetworkMessage>())) {

            }
        }

        void CreateInitMethod() {
            const MethodAttributes methodAttr = MethodAttributes.Public | MethodAttributes.Static;
            var method = new MethodDefinition("SerializationInit", methodAttr, Module.ImportReference(typeof(void)));
            var processor = method.Body.GetILProcessor();

            // add attribute
            method.CustomAttributes.Add(CreateAttrbute<RuntimeInitializeOnLoadMethodAttribute>(RuntimeInitializeLoadType.BeforeSceneLoad));

            InitializeWriters(processor);
            InitializeReaders(processor);
        }

        #region Write/Read blah blah blah

        void InitializeWriters(ILProcessor processor) {
            // references
            var actionRef = Module.ImportReference(typeof(Action<,>));
            var actionCtorRef = Module.ImportReference(typeof(Action<,>).GetConstructors()[0]);
            var writerRef = Module.ImportReference(typeof(Writer));
            var genericWriterRef = Module.ImportReference(typeof(Writer<>));
            var writerFieldRef = Module.ImportReference(typeof(Writer<>).GetField(nameof(Writer<object>.write)));

            // code
            foreach (var write in writers) {
                var target = write.Parameters[1].ParameterType;

                // create delegate
                var instance = actionRef.MakeGenericInstanceType(writerRef, target);
                var refInstance = Module.ImportReference(actionCtorRef.MakeHostInstanceGeneric(instance));

                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ldftn, write);
                processor.Emit(OpCodes.Newobj, refInstance);

                // save
                var genericInstance = genericWriterRef.MakeGenericInstanceType(target);
                var field = Module.ImportReference(writerFieldRef.SpecializeField(genericInstance));
                processor.Emit(OpCodes.Stsfld, field);
            }
        }

        void InitializeReaders(ILProcessor processor) {
            // references
            var funcRef = Module.ImportReference(typeof(Func<,>));
            var funcCtorRef = Module.ImportReference(typeof(Func<,>).GetConstructors()[0]);
            var readerRef = Module.ImportReference(typeof(Reader));
            var genericReaderRef = Module.ImportReference(typeof(Reader<>));
            var readerFieldRef = Module.ImportReference(typeof(Reader<>).GetField(nameof(Reader<object>.read)));

            // code
            foreach (var read in readers) {
                var target = read.ReturnType;

                // create delegate
                var instance = funcRef.MakeGenericInstanceType(readerRef, target);
                var refInstance = Module.ImportReference(funcCtorRef.MakeHostInstanceGeneric(instance));

                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ldftn, read);
                processor.Emit(OpCodes.Newobj, refInstance);

                // save
                var genericInstance = genericReaderRef.MakeGenericInstanceType(target);
                var field = Module.ImportReference(readerFieldRef.SpecializeField(genericInstance));
                processor.Emit(OpCodes.Stsfld, field);
            }
        }

        void GetAllWriters(TypeDefinition t) {
            foreach (MethodDefinition method in t.Methods) {
                if (method.Parameters.Count == 3) {
                    if (!method.Parameters[2].ParameterType.Is<int>()) continue;
                } else if (method.Parameters.Count != 2) {
                    continue;
                }

                if (!method.ReturnType.Is(typeof(void))) continue;
                if (!method.Parameters[0].ParameterType.Is<Writer>()) continue;
                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>()) continue;
                if (method.HasGenericParameters) continue;

                writers.Add(method);
            }
        }

        void GetAllReaders(TypeDefinition t) {
            foreach (MethodDefinition method in t.Methods) {
                if (method.Parameters.Count == 2) {
                    if (!method.Parameters[1].ParameterType.Is<int>()) continue;
                } else if (method.Parameters.Count != 1) {
                    continue;
                }

                if (method.ReturnType.Is(typeof(void))) continue;
                if (!method.Parameters[0].ParameterType.Is<Reader>()) continue;
                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>()) continue;
                if (method.HasGenericParameters) continue;

                readers.Add(method);
            }
        }

        #endregion

        #region Message Functions



        #endregion
    }
}
