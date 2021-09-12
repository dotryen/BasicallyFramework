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
    }
}
