using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Basically.Editor.Weaver {
    using Networking;

    internal class ReceiverWeaver : Weaver {
        public override int Priority => 0;
        public override bool IsEditor => false;

        public override void Weave() {
            var addRef = Module.ImportReference(typeof(NetworkHost).GetMethod(nameof(NetworkHost.AddReceiver)));

            foreach (var type in Module.Types.Where(x => x.IsSealed && x.IsAbstract)) {
                if (!type.HasCustomAttribute<ReceiverClassAttribute>(out var attr)) continue;

                var sysType = type.GetSystemType();
                var init = CreateInitMethod(type);
                var worker = init.Body.GetILProcessor();

                foreach (var method in type.Methods.Where(x => x.IsPublic && x.IsStatic && x.Parameters.Count == 2)) {
                    if (!method.Parameters[0].ParameterType.Is<Connection>()) continue;
                    if (!method.Parameters[1].ParameterType.Implements<NetworkMessage>()) continue;

                    var noAuth = method.HasCustomAttribute<NoAuthAttribute>();
                    AddReceiver(worker, method, sysType.GetMethod(method.Name).GetParameters()[1].ParameterType, noAuth);
                }

                worker.Emit(OpCodes.Ret);

                type.Methods.Add(init);
            }
        }

        MethodDefinition CreateInitMethod(TypeDefinition type) {
            const MethodAttributes ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var method = new MethodDefinition("_Init", ATTRIBUTES, Module.ImportReference(typeof(void)));

            method.Parameters.Add(new ParameterDefinition("host", ParameterAttributes.None, Module.ImportReference(typeof(NetworkHost))));
            method.Body.InitLocals = true;

            return method;
        }

        void AddReceiver(ILProcessor worker, MethodDefinition method, Type parameter, bool noAuth) {
            var actionRef = Module.ImportReference(typeof(Action<,>));
            var actionCtorRef = Module.ImportReference(typeof(Action<,>).GetConstructors()[0]);
            var connRef = Module.ImportReference(typeof(Connection));
            var messageRef = method.Parameters[1].ParameterType;

            // create action
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldnull);
            worker.Emit(OpCodes.Ldftn, Module.ImportReference(method));

            var genericInst = actionRef.MakeGenericInstanceType(connRef, messageRef);
            worker.Emit(OpCodes.Newobj, actionCtorRef.MakeHostInstanceGeneric(genericInst, Module));

            // add to receiver
            worker.Emit(noAuth ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            var add = Module.ImportReference(typeof(NetworkHost).GetMethod(nameof(NetworkHost.AddReceiver)).MakeGenericMethod(parameter));
            worker.Emit(OpCodes.Callvirt, add);

            worker.Emit(OpCodes.Nop);
        }
    }
}
