using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Basically.Serialization {
    public class StringSerializer : Serializer<string> {
        public override void Write(Buffer buffer, string value) {
            buffer.Write(Encoding.UTF8.GetBytes(value));
        }

        public override string Read(Buffer buffer) {
            return Encoding.UTF8.GetString(buffer.ReadByteArray());
        }
    }
}
