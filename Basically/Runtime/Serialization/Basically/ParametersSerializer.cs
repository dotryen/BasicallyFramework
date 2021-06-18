using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Utility;

namespace Basically.Serialization {
    public class ParametersSerializer : Serializer<Parameters> {
        public override void Write(Buffer buffer, Parameters value) {
            buffer.Write(value.Count);

            var stringSerial = GetSerializer<string>();

            foreach (var pair in value) {
                var valueSerial = GetSerializer(pair.Value.GetType());
                if (valueSerial == null) continue; // object has no serializer, why bother

                buffer.Write(valueSerial.Index, SerializerStorage.SerializerBits);
                stringSerial.Write(buffer, pair.Key);
                valueSerial.WriteInternal(buffer, pair.Value);
            }
        }

        public override Parameters Read(Buffer buffer) {
            byte count = buffer.ReadByte();
            var stringSerial = GetSerializer<string>();
            Parameters param = new Parameters(count);

            for (byte i = 0; i < count; i++) {
                var valueSerial = GetSerializer(buffer.ReadByte(SerializerStorage.SerializerBits));
                param.Add(stringSerial.Read(buffer), valueSerial.ReadInternal(buffer));
            }

            return param;
        }
    }
}
