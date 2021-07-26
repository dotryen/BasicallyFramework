// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Mono.Cecil;
// using Mono.Cecil.Cil;
// 
// namespace Basically.Editor.Weaver {
//     internal class CompilerWeaver : Weaver {
//         public override bool IsEditor => true;
//         public override int Priority => 0;
// 
//         public TypeDefinition storageType;
//         public PropertyDefinition storageProp;
//         public MethodDefinition saveMethod;
// 
//         const PropertyAttributes PROP_ATTRIBUTES = PropertyAttributes.None;
//         const MethodAttributes METH_ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName;
// 
//         public override void Weave() {
//             storageType = Module.GetType("CompilerStorage");
//             if (storageType == null) return;
// 
//             storageProp = storageType.Properties[0];
//             saveMethod = storageType.Methods.First(x => x.Name == "Save");
// 
//             foreach (var field in storageType.Fields) {
//                 CreateProperty(field);
//             }
//         }
// 
//         void CreateProperty(FieldDefinition field) {
//             if (!HasAttribute<UnityEngine.HeaderAttribute>(field, out var header)) return;
// 
//             var boolRef = Module.ImportReference(typeof(bool));
//             var prop = new PropertyDefinition($"Gen_{field.Name}", PROP_ATTRIBUTES,  boolRef);
//             prop.CustomAttributes.Add(CreateAttrbute<MenuCheckboxAttribute>("Basically/" + (string)header.ConstructorArguments[0].Value));
// 
//             // Create get
//             {
//                 var get = new MethodDefinition($"get_{prop.Name}", METH_ATTRIBUTES, boolRef);
//                 var process = get.Body.GetILProcessor();
// 
//                 process.Emit(OpCodes.Nop);
//                 process.Emit(OpCodes.Call, storageProp.GetMethod);
//                 process.Emit(OpCodes.Ldfld, field);
//                 process.Emit(OpCodes.Stloc_0);
//                 process.Emit(OpCodes.Ret);
// 
//                 prop.GetMethod = get;
//             }
// 
//             {
//                 var set = new MethodDefinition($"set_{prop.Name}", METH_ATTRIBUTES, boolRef);
//                 var process = set.Body.GetILProcessor();
// 
//                 process.Emit(OpCodes.Nop);
//                 process.Emit(OpCodes.Call, storageProp.GetMethod);
//                 process.Emit(OpCodes.Stloc_0);
//                 process.Emit(OpCodes.Ldloc_0);
//                 process.Emit(OpCodes.Ldarg_0);
//                 process.Emit(OpCodes.Stfld, field);
//                 process.Emit(OpCodes.Ldloc_0);
//                 process.Emit(OpCodes.Callvirt, saveMethod);
//                 process.Emit(OpCodes.Nop);
//                 process.Emit(OpCodes.Ret);
// 
//                 prop.GetMethod = set;
//             }
// 
//             storageType.Properties.Add(prop);
//         }
//     }
// }
