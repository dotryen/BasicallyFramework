using System;

namespace Basically.Editor {

    /// <summary>
    /// Similar to MenuItem but creates a checkbox.
    /// NOTE: Must be static and a bool or it will be ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class MenuCheckboxAttribute : Attribute {
        public string name;

        public MenuCheckboxAttribute(string name) {
            this.name = name;
        }
    }
}
