using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Serialization {
    public class Vector3Serializer : Serializer<Vector3> {
        public override void Write(Buffer buffer, Vector3 value) {
            buffer.Write(value.x);
            buffer.Write(value.y);
            buffer.Write(value.z);
        }

        public override Vector3 Read(Buffer buffer) {
            return new Vector3(buffer.ReadFloat(), buffer.ReadFloat(), buffer.ReadFloat());
        }
    }
}
