using CopperMatchmaking.Data;
using CopperMatchmaking.Info;
using Riptide;

namespace CopperMatchmaking.Server
{
    internal static class ServerMessageHandlers
    {
        [MessageHandler((ushort)MessageIds.ClientJoined)]
        internal static void ClientJoinedMessageHandler(ushort sender, Message receivedMessage)
        {
            var playerId = receivedMessage.GetULong();
            var rankId = receivedMessage.GetByte();

            Log.Info($"Received new ClientJoined message. | PlayerId: {playerId} | RankId: {rankId} | Sender: {sender}");

            var connection = MatchmakerServer.Instance.Server.Clients[sender - 1];
            var rank = MatchmakerServer.Instance.Ranks[rankId - 1];

            MatchmakerServer.Instance.RegisterClient(new ConnectedClient(rank, connection));
        }

        [MessageHandler((ushort)MessageIds.ClientHostLobbyId)]
        internal static void ClientHostLobbyIdMessageHandler(ushort sender, Message receivedMessage)
        {
            var lobbyId = receivedMessage.GetUInt();
            var hostedLobbyId = receivedMessage.GetULong();

            Log.Info($"Received new ClientHostLobbyId message. | LobbyId: {lobbyId} | HostedLobbyId: {hostedLobbyId}");

            MatchmakerServer.Instance.LobbyManager.HandleClientHostResponse(lobbyId, hostedLobbyId);
        }
    }
}