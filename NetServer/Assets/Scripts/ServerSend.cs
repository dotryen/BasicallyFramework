using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using ENet;
// using EPacket = ENet.Packet;

public static class ServerSend {
    /*
    internal static EPacket BuildEPacket(Packet _packet, PacketFlags flags) {
        var buffer = _packet.ToArray();
        _packet.Dispose();
    
        if (buffer[0] > (byte)Enum.GetValues(typeof(ServerPackets)).Cast<ServerPackets>().Last())
            throw new Exception($"Packet ID {buffer[0]} does not exist!");
    
        EPacket packet = default;
        packet.Create(buffer, flags);
        return packet;
    }

    #region Reliable

    private static void SendReliable(uint _toClient, Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Reliable);

        if (!NetworkServer.clients[_toClient].peer.Send((byte)ENetChannels.Reliable, ref packet)) {
            Debug.LogError($"Failed to send packet. Client: {_toClient}, SendReliable");
        }
    }
    private static void SendReliableToAll(Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Reliable);

        NetworkServer.host.Broadcast((byte)ENetChannels.Reliable, ref packet);
    }
    private static void SendReliableToAll(uint _exceptClient, Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Reliable);

        NetworkServer.host.Broadcast((byte)ENetChannels.Reliable, ref packet, NetworkServer.clients[_exceptClient].peer);
    }

    #endregion

    #region Unreliable

    private static void Send(uint _toClient, Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Unsequenced);

        if (!NetworkServer.clients[_toClient].peer.Send((byte)ENetChannels.Unreliable, ref packet)) {
            Debug.LogError($"Failed to send packet. Client: {_toClient}, SendUnreliable");
        }
    }
    private static void SendToAll(Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Unsequenced);

        NetworkServer.host.Broadcast((byte)ENetChannels.Unreliable, ref packet);
    }
    private static void SendToAll(uint _exceptClient, Packet _packet) {
        EPacket packet = BuildEPacket(_packet, PacketFlags.Unsequenced);

        NetworkServer.host.Broadcast((byte)ENetChannels.Unreliable, ref packet, NetworkServer.clients[_exceptClient].peer);
    }

    #endregion
    

    #region Packets

    // Where authentication takes place
    public static void Welcome(uint _toClient) {
        Packet _packet = new Packet((byte)ServerPackets.Welcome);

        _packet.Write(Master.Instance.tick);

        SendReliable(_toClient, _packet);
    }

    public static void Update() {
        Packet _packet = new Packet((byte)ServerPackets.Update);
        _packet.Write(Master.Instance.tick);

        var states = EntityManager.CreateStates();

        for (int i = 0; i < states.Length; i++)  {
            _packet.Write(states[i].id);
            _packet.Write(states[i].state.position);
            _packet.Write(states[i].state.rotation);
            _packet.Write(states[i].state.parameters);
        }

        SendToAll(_packet);
    }

#endregion
    */
}
