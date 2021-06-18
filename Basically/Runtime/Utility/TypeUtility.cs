using System;
using System.Collections;
using System.Collections.Generic;

namespace Basically.Utility {
    public static class TypeUtility {
        public static bool IsBaseType(Type baseType, Type tested) {
            if (baseType.IsSealed) return false; // Cannot be inherited
            return baseType.IsAssignableFrom(tested);
        }
    }
}
