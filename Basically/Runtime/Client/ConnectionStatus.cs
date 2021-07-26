using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basically.Client {
    public enum ConnectionStatus {
        NotConnected,
        WaitingForHandshake,
        Connected
    }
}