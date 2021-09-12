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

        WriterProcessor writerProcessor;
        ReaderProcessor readerProcessor;

        public override void Weave() {
            if (readerProcessor == null) {
                readerProcessor = new ReaderProcessor(Module, GeneratedCodeClass);
            } else {
                readerProcessor.Reset(Module, GeneratedCodeClass);
            }

            if (writerProcessor == null) {
                writerProcessor = new WriterProcessor(Module, GeneratedCodeClass);
            } else {
                writerProcessor.Reset(Module, GeneratedCodeClass);
            }

            foreach (var type in Module.Types.Where(x => x.IsSealed && x.IsAbstract)) {
                GetAllWriters(type);
                GetAllReaders(type);
            }

            foreach (var type in Module.Types.Where(x => !x.IsAbstract && !x.IsInterface && x.Implements<NetworkMessage>())) {
                writerProcessor.GetWriteFunc(type);
                readerProcessor.GetReadFunc(type);
            }

            // foreach (var ent in Module.Types.Where(x => x.Inherits<Entities.Entity>())) {
            // 
            // }

            var method = CreateInitMethod();
            var worker = method.Body.GetILProcessor();

            writerProcessor.InitializeWriters(worker);
            readerProcessor.InitializeReaders(worker);
            worker.Emit(OpCodes.Ret);

            GeneratedCodeClass.Methods.Add(method);
        }

        MethodDefinition CreateInitMethod() {
            const MethodAttributes methodAttr = MethodAttributes.Public | MethodAttributes.Static;
            var method = new MethodDefinition("SerializationInit", methodAttr, Module.ImportReference(typeof(void)));
            method.CustomAttributes.Add(CreateAttrbute<RuntimeInitializeOnLoadMethodAttribute>(RuntimeInitializeLoadType.BeforeSceneLoad));

            return method;
        }

        void GetAllWriters(TypeDefinition t) {
            foreach (MethodDefinition method in t.Methods) {
                bool bit = false;
                if (method.Parameters.Count == 3) {
                    var par = method.Parameters[2];
                    if (!par.ParameterType.Is<int>()) continue;
                    if (!par.IsOptional) continue;
                    bit = true;
                } else if (method.Parameters.Count != 2) {
                    continue;
                }

                if (!method.ReturnType.Is(typeof(void))) continue;
                if (!method.Parameters[0].ParameterType.Is<Writer>()) continue;
                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>()) continue;
                if (method.HasGenericParameters) continue;

                writerProcessor.Register(method.Parameters[1].ParameterType, Module.ImportReference(method), bit);
            }
        }

        void GetAllReaders(TypeDefinition t) {
            foreach (MethodDefinition method in t.Methods) {
                bool bit = false;

                if (method.Parameters.Count == 2) {
                    if (!method.Parameters[1].ParameterType.Is<int>()) continue;
                    bit = true;
                } else if (method.Parameters.Count != 1) {
                    continue;
                }

                if (!method.Parameters[0].ParameterType.Is<Reader>()) continue;
                if (!method.HasCustomAttribute<System.Runtime.CompilerServices.ExtensionAttribute>()) continue;
                if (method.HasGenericParameters) continue;

                readerProcessor.Register(method.ReturnType, Module.ImportReference(method), bit);
            }
        }
    }
}
