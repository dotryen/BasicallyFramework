using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Basically.Serialization;
using Basically.Networking;
using Basically.Networking.ENet;
using Buffer = Basically.Serialization.Buffer;
using emotitron.Compression;

namespace Tests
{
    public class SerializerTester
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Vector3SerializerTest() {
            SerializerStorage.Initialize();
            var serial = SerializerStorage.GetSerializer<Vector3>();
            var vector = new Vector3(0, 1, 0);
            Buffer buffer = new Buffer();

            serial.Write(buffer, vector);
            var read = serial.Read(buffer);

            Assert.AreEqual(vector, read);
        }

        // [Test]
        // public void MessageTest() {
        //     void PrintTest(testtype testtype) {
        //         Debug.Log($"One: {testtype.one}");
        //         Debug.Log($"Two: {testtype.two}");
        //     }
        // 
        //     SerializerStorage.Initialize();
        // 
        //     var message = new TestMessage();
        //     message.type = new testtype() {
        //         one = 1,
        //         two = "fuck"
        //     };
        // 
        //     var arr = MessagePacker.SerializeMessage(message);
        //     var newMessage = (TestMessage)MessagePacker.DeserializeMessage(arr);
        // 
        //     PrintTest(message.type);
        //     PrintTest(newMessage.type);
        // 
        //     Assert.AreEqual(message.type.one, newMessage.type.one);
        //     Assert.AreEqual(message.type.two, newMessage.type.two);
        // }

        [Test]
        public void QuaternionSerialize() {
            SerializerStorage.Initialize();
            var serial = SerializerStorage.GetSerializer<Quaternion>();

            Buffer buffer = new Buffer();
            Quaternion value = new Quaternion(1, 3, 4, 2).normalized;

            serial.Write(buffer, value);
            var decompressed = serial.Read(buffer);

            Assert.That(decompressed.x, Is.EqualTo(value.x).Within(0.005f));
            Assert.That(decompressed.y, Is.EqualTo(value.y).Within(0.005f));
            Assert.That(decompressed.z, Is.EqualTo(value.z).Within(0.005f));
            Assert.That(decompressed.w, Is.EqualTo(value.w).Within(0.005f));
        }
    }
}
