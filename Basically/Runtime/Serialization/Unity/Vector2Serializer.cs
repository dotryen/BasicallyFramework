using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Serialization {
    public class Vector2Serializer : Serializer<Vector2> {
        public override void Write(Buffer buffer, Vector2 value) {
            buffer.Write(value.x);
            buffer.Write(value.y);
        }

        public override Vector2 Read(Buffer buffer) {
            return new Vector2(buffer.ReadFloat(), buffer.ReadFloat());
        }
    }
}
