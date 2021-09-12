using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Serialization;

namespace Basically.Networking {
    internal delegate void MessageDelegate(Connection conn, Reader reader);
}
