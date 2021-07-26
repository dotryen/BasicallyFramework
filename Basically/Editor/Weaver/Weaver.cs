using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Basically.Editor.Weaver {
    public abstract class Weaver {
        public abstract bool IsEditor { get; }
        public virtual int Priority => 0;
        public TypeDefinition GeneratedCodeClass => WeaverMaster.CurrentGeneratedClass;
        public ModuleDefinition Module { get; internal set; }

        /// <summary>
        /// Weaves the current assembly (Module)
        /// </summary>
        public abstract void Weave();
        
        /// <summary>
        /// Can be used to reset any values after compilation
        /// </summary>
        public virtual void Reset() {
            // not required
        }

        #region Helpers

        #region Attributes

        public bool HasAttribute<T>(FieldDefinition field, out CustomAttribute attribute) where T : System.Attribute {
            if (!field.HasCustomAttributes) {
                attribute = null;
                return false;
            }

            string name = typeof(T).FullName;
            foreach (var attr in field.CustomAttributes) {
                if (attr.AttributeType.FullName != name) continue;

                attribute = attr;
                return true;
            }

            attribute = null;
            return false;
        }

        public bool HasAttribute<T>(PropertyDefinition property, out CustomAttribute attribute) where T : System.Attribute {
            if (!property.HasCustomAttributes) {
                attribute = null;
                return false;
            }

            string name = typeof(T).FullName;
            foreach (var attr in property.CustomAttributes) {
                if (attr.AttributeType.FullName != name) continue;

                attribute = attr;
                return true;
            }

            attribute = null;
            return false;
        }

        public bool HasAttribute<T>(MethodDefinition method, out CustomAttribute attribute) where T : System.Attribute {
            if (!method.HasCustomAttributes) {
                attribute = null;
                return false;
            }

            string name = typeof(T).FullName;
            foreach (var attr in method.CustomAttributes) {
                if (attr.AttributeType.FullName != name) continue;

                attribute = attr;
                return true;
            }

            attribute = null;
            return false;
        }

        public CustomAttribute CreateAttrbute<T>(params object[] args) where T : Attribute {
            var con = typeof(T).GetConstructor(args.Select(x => x.GetType()).ToArray());
            if (con == null) throw new ArgumentException("Arguments are invalid.", "args");

            var conRef = Module.ImportReference(con);
            CustomAttribute attr = new CustomAttribute(conRef);
            for (int i = 0; i < args.Length; i++) {
                attr.ConstructorArguments.Add(new CustomAttributeArgument(Module.ImportReference(args[i].GetType()), args[i]));
            }

            return attr;
        }

        #endregion

        #region Types

        public TypeDefinition[] GetDescendants<T>() {
            return GetDescendants(typeof(T));
        }

        public TypeDefinition[] GetDescendants(Type type) {
            if (type.IsInterface) {
                return Module.Types.Where((x) => {
                    if (x.FullName == "<Module>") return false;
                    return x.Interfaces.Any(y => y.InterfaceType.FullName == type.FullName);
                }).ToArray();
            } else {
                return Module.Types.Where((x) => {
                    if (x.FullName == "<Module>") return false;
                    if (x.BaseType == null) return false;
                    return x.BaseType.FullName.StartsWith(type.FullName);
                }).ToArray();
            }
        }

        public TypeDefinition GetDescendant<T>() {
            return GetDescendant(typeof(T));
        }

        public TypeDefinition GetDescendant(Type type) {
            var types = GetDescendants(type);
            return types.Length != 0 ? types[0] : null;
        }

        public TypeDefinition GetType<T>() {
            return Module.Types.First(x => x.FullName == typeof(T).FullName);
        }

        #endregion

        #endregion
    }
}
