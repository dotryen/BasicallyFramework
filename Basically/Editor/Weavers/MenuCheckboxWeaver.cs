using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using riles.Weaver;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace Basically.Editor {
    public class MenuCheckboxWeaver : Weaver {
        public override bool IsEditor => true;
        public override int Priority => 0;

        public override void Weave() {
            string attributeName = typeof(MenuCheckboxAttribute).FullName;
            string boolFull = typeof(bool).FullName;

            List<FieldDefinition> fields = new List<FieldDefinition>();
            List<PropertyDefinition> properties = new List<PropertyDefinition>();

            foreach (var type in Module.Types) {
                if (type.Name == "<Module>") continue;
                fields.AddRange(type.Fields.Where(x => x.IsStatic && x.HasCustomAttributes && x.FieldType.FullName == boolFull));

                properties.AddRange(type.Properties.Where(y => {
                    if (y.SetMethod == null) return false;
                    var set = y.SetMethod;
                    return set.IsStatic && y.HasCustomAttributes && y.PropertyType.FullName == boolFull;
                }));
            }

            if (fields.Count + properties.Count == 0) return;

            // create shit
            foreach (var field in fields) {
                if (field.HasCustomAttribute<MenuCheckboxAttribute>(out var attr)) {

                    var name = (string)attr.ConstructorArguments[0].Value;
                    GeneratedCodeClass.Methods.Add(CreateButton(name, field));
                    GeneratedCodeClass.Methods.Add(CreateValidate(name, field));
                }
            }

            foreach (var property in properties) {
                if (property.HasCustomAttribute<MenuCheckboxAttribute>(out var attr)) {

                    var name = (string)attr.ConstructorArguments[0].Value;
                    GeneratedCodeClass.Methods.Add(CreateButton(name, property));
                    GeneratedCodeClass.Methods.Add(CreateValidate(name, property));
                }
            }
        }

        const MethodAttributes ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

        MethodDefinition CreateButton(string name, FieldDefinition field) {
            var def = new MethodDefinition($"_Check_{field.DeclaringType.Name}_{field.Name}_A", ATTRIBUTES, Module.ImportReference(typeof(void)));
            def.CustomAttributes.Add(MenuItemAttribute(name, false));

            // This deals with IL, its gonna get messy
            var processor = def.Body.GetILProcessor();

            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Ldsfld, field); // Gets the value of a static field
            processor.Emit(OpCodes.Ldc_I4_0); // Pushes 0
            processor.Emit(OpCodes.Ceq); // Compares
            processor.Emit(OpCodes.Stsfld, field); // Pushes value back to static field
            processor.Emit(OpCodes.Ret); // Returns

            return def;
        }

        MethodDefinition CreateButton(string name, PropertyDefinition property) {
            var def = new MethodDefinition($"_Check_{property.DeclaringType.Name}_{property.Name}_A", ATTRIBUTES, Module.ImportReference(typeof(void)));
            def.CustomAttributes.Add(MenuItemAttribute(name, false));

            // This deals with IL, its gonna get messy
            var processor = def.Body.GetILProcessor();

            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Call, property.GetMethod);
            processor.Emit(OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Ceq);
            processor.Emit(OpCodes.Call, property.SetMethod);
            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Ret);

            return def;
        }

        MethodDefinition CreateValidate(string name, FieldDefinition field) {
            var def = new MethodDefinition($"_Check_{field.DeclaringType.Name}_{field.Name}_B", ATTRIBUTES, Module.ImportReference(typeof(bool)));
            def.CustomAttributes.Add(MenuItemAttribute(name, true));

            // This deals with IL, its gonna get messy
            var processor = def.Body.GetILProcessor();

            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Ldstr, name);
            processor.Emit(OpCodes.Ldsfld, field);
            processor.Emit(OpCodes.Call, Module.ImportReference(typeof(UnityEditor.Menu).GetMethod("SetChecked", new Type[] { typeof(string), typeof(bool) })));
            processor.Emit(OpCodes.Nop);

            processor.Emit(OpCodes.Ldc_I4_1);
            processor.Emit(OpCodes.Ret);

            return def;
        }

        MethodDefinition CreateValidate(string name, PropertyDefinition property) {
            var def = new MethodDefinition($"_Check_{property.DeclaringType.Name}_{property.Name}_B", ATTRIBUTES, Module.ImportReference(typeof(bool)));
            def.CustomAttributes.Add(MenuItemAttribute(name, true));

            // This deals with IL, its gonna get messy
            var processor = def.Body.GetILProcessor();

            processor.Emit(OpCodes.Nop);
            processor.Emit(OpCodes.Ldstr, name);
            processor.Emit(OpCodes.Call, property.GetMethod);
            processor.Emit(OpCodes.Call, Module.ImportReference(typeof(UnityEditor.Menu).GetMethod("SetChecked", new Type[] { typeof(string), typeof(bool) })));
            processor.Emit(OpCodes.Nop);

            processor.Emit(OpCodes.Ldc_I4_1);
            processor.Emit(OpCodes.Ret);

            return def;
        }

        string GenerateName(string declaring, string name, char ver) {
            return $"_Check_{declaring}_{name}_{ver}";
        }

        CustomAttribute MenuItemAttribute(string name, bool validate) {
            return CreateAttrbute<UnityEditor.MenuItem>(name, validate);
        }
    }
}
