using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Basically.Editor {
    using Utility;

    public static class TypeUtility {
        public static IEnumerable<Type> GetAllDescendantsOf(this Assembly assembly, Type type) {
            return assembly.GetTypes().Where(x => type.IsAssignableFrom(x) && x != type);
        }

        public static IEnumerable<Type> GetAllGenericDescendantsOf(this Assembly assembly, Type genericTypeDefinition) {
            return from x in assembly.GetTypes()
                   let y = x.BaseType
                   where !x.IsAbstract && !x.IsInterface && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == genericTypeDefinition
                   select x;
        }
    }
}
