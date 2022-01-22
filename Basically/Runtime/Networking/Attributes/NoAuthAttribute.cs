using System;

namespace Basically.Networking {
    /// <summary>
    /// Indicates that the handler does not require an authenticated connection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NoAuthAttribute : Attribute {

    }
}
