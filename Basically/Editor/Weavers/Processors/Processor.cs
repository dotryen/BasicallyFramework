using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using riles.Weaver;

namespace Basically.Editor {
    internal abstract class Processor {
        Dictionary<TypeReference, MethodReference> all;
        Dictionary<TypeReference, MethodReference> allBit;
        Dictionary<TypeReference, MethodReference> allDelta;

        Dictionary<TypeReference, MethodReference> current;
        Dictionary<TypeReference, MethodReference> currentBit;
        Dictionary<TypeReference, MethodReference> currentDelta;

        ModuleDefinition mainModule;
        TypeDefinition genClass;

        private Processor() {
            all = CreateDictionary();
            allBit = CreateDictionary();
            allDelta = CreateDictionary();

            currentDelta = CreateDictionary();
            currentBit = CreateDictionary();
            current = CreateDictionary();
        }

        public Processor(ModuleDefinition mod, TypeDefinition gen) : this() {
            Reset(mod, gen);
        }

        private Dictionary<TypeReference, MethodReference> CreateDictionary() {
            return new Dictionary<TypeReference, MethodReference>(new TypeReferenceComparer());
        }

        public void Reset(ModuleDefinition mod, TypeDefinition gen) {
            mainModule = mod;
            genClass = gen;
            current.Clear();
            currentBit.Clear();

            if (all.Count != 0) { // import regular
                var imported = CreateDictionary();
                foreach (var pair in all) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                all = imported;
            }

            if (allBit.Count != 0) { // import bit
                var imported = CreateDictionary();
                foreach (var pair in allBit) {
                    imported.Add(mainModule.ImportReference(pair.Key), mainModule.ImportReference(pair.Value));
                }
                allBit = imported;
            }
        }

        public void Register(TypeReference dataType, MethodReference method, bool bit = false) {
            if (all.ContainsKey(dataType)) {
                return;
            }

            // we need to import type when we Initialize Writers so import here in case it is used anywhere else
            TypeReference imported = mainModule.ImportReference(dataType);

            if (bit) {
                allBit[imported] = method;
                currentBit[imported] = method;

                // GenerateBitToRegular(imported, method); // creates a method that calls the bit version from regular calls
            } else {
                all[imported] = method;
                current[imported] = method;
            }
        }

        void RegisterFunc(TypeReference typeReference, MethodDefinition newWriterFunc) {
            Register(typeReference, newWriterFunc);

            genClass.Methods.Add(newWriterFunc);
        }

        public abstract void BitToRegular();
    }

    internal enum MethodType { Normal, Bit, Delta }
}
