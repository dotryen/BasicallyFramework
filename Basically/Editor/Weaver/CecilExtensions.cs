using System;
using System.Linq;
using Mono.Cecil;

namespace Basically.Editor.Weaver {
    public static class CecilExtensions {
        public static bool Is(this TypeReference td, Type type) {
            if (type.IsGenericType) {
                return td.GetElementType().FullName == td.FullName;
            }
            return td.FullName == type.FullName;
        }

        public static bool Is<T>(this TypeReference td) {
            return Is(td, typeof(T));
        }

        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute {
            return provider.CustomAttributes.Any(x => x.AttributeType.Is<T>());
        }

        public static bool Implements<T>(this TypeDefinition td) {
            TypeDefinition def = td;

            while (def != null) {
                if (def.Interfaces.Any(x => x.InterfaceType.Is<T>())) return true;

                try {
                    def = def.BaseType?.Resolve();
                } catch (AssemblyResolutionException) {
                    break;
                }
            }

            return false;
        }

        public static bool Inherits<T>(this TypeReference td) => Inherits(td.Resolve(), typeof(T));

        public static bool Inherits(this TypeReference td, Type baseClass) => Inherits(td.Resolve(), baseClass);

        public static bool Inherits<T>(this TypeDefinition td) => Inherits(td, typeof(T));

        public static bool Inherits(this TypeDefinition td, Type baseClass) {
            if (!td.IsClass) return false;

            TypeReference parent = td.BaseType;
            if (parent == null) return false;
            if (parent.Is(baseClass)) return true;
            if (parent.CanBeResolved()) return Inherits(parent.Resolve(), baseClass);

            return false;
        }

        public static bool CanBeResolved(this TypeReference parent) {
            while (parent != null) {
                if (parent.Scope.Name == "Windows") return false;
                if (parent.Scope.Name == "mscorlib") return parent.Resolve() != null;

                try {
                    parent = parent.Resolve().BaseType;
                } catch {
                    return false;
                }
            }
            return true;
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, GenericInstanceType type) {
            MethodReference refer = new MethodReference(self.Name, self.ReturnType, type) {
                CallingConvention = self.CallingConvention,
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis
            };

            foreach (var param in self.Parameters) {
                refer.Parameters.Add(new ParameterDefinition(param.ParameterType));
            }

            foreach (var genParam in self.GenericParameters) {
                refer.GenericParameters.Add(new GenericParameter(genParam.Name, refer));
            }

            return refer;
        }

        public static FieldReference SpecializeField(this FieldReference self, GenericInstanceType type) {
            return new FieldReference(self.Name, self.FieldType, type);
        }

        public static bool IsMultidimensionalArray(this TypeReference type) {
            return type is ArrayType arrayType && arrayType.Rank > 1;
        }
    }
}
