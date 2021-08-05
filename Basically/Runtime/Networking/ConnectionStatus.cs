namespace Basically.Networking {
    using ENet;

    public enum ConnectionStatus {
        NotConnected = PeerState.Disconnected,
        Connecting = PeerState.Connected | PeerState.AcknowledgingConnect | PeerState.ConnectionPending,
        WaitingForHandshake = PeerState.Connected,
        Connected = PeerState.Connected,
        Disconnecting = PeerState.DisconnectLater | PeerState.Disconnecting | PeerState.AcknowledgingDisconnect,
    }
}