using System;
using System.Collections;
using System.Collections.Generic;

// god mirror does a lot of clever things
namespace Basically.Serialization {
    using Networking;

    /// <summary>
    /// Class that contains writers for messages.
    /// </summary>
    /// <typeparam name="T">Message type.</typeparam>
    internal static class NetworkWriter<T> where T : struct, NetworkMessage {
        public static Action<Buffer, T> write;
    }
}
