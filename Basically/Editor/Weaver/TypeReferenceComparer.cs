using System.Collections.Generic;
using Mono.Cecil;

namespace Basically.Editor.Weaver {
    public class TypeReferenceComparer : IEqualityComparer<TypeReference> {
        public bool Equals(TypeReference x, TypeReference y) {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(TypeReference obj) {
            return obj.FullName.GetHashCode();
        }
    }
}
