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

        // System.Reflection.MethodInfo addReceiverInfo;
        MethodReference addReceiverInfo;

        public override void Weave() {
            addReceiverInfo = Module.ImportReference(typeof(MethodHandler).GetMethod(nameof(MethodHandler.AddReceiver)));

            foreach (var type in Module.Types.Where(x => x.IsSealed && x.IsAbstract)) {
                if (!type.HasCustomAttribute<ReceiverClassAttribute>(out var attr)) continue;

                var init = CreateInitMethod();
                var worker = init.Body.GetILProcessor();

                foreach (var method in type.Methods.Where(x => x.IsPublic && x.IsStatic && x.Parameters.Count == 2)) {
                    if (!method.Parameters[0].ParameterType.Is<Connection>()) continue;
                    if (!method.Parameters[1].ParameterType.Implements<NetworkMessage>()) continue;

                    var noAuth = method.HasCustomAttribute<NoAuthAttribute>();
                    AddReceiver(worker, method, noAuth);
                }

                worker.Emit(OpCodes.Ret);

                type.Methods.Add(init);
            }
        }

        MethodDefinition CreateInitMethod() {
            const MethodAttributes ATTRIBUTES = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var method = new MethodDefinition("_Init", ATTRIBUTES, Module.ImportReference(typeof(void)));

            method.Parameters.Add(new ParameterDefinition("handler", ParameterAttributes.None, Module.ImportReference(typeof(MethodHandler))));
            method.Body.InitLocals = true;

            return method;
        }

        void AddReceiver(ILProcessor worker, MethodDefinition method, bool noAuth) {
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
            var add = Module.ImportReference(addReceiverInfo.MakeGenericMethod(messageRef));
            worker.Emit(OpCodes.Callvirt, add);

            worker.Emit(OpCodes.Nop);
        }
    }
}
