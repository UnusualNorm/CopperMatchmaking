using System;
using CopperMatchmaking.Data;
using CopperMatchmaking.Info;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;
using RiptideClient = Riptide.Client;

namespace CopperMatchmaking.Client
{
    /// <summary>
    /// Matchmaker client for connecting to the matchmaker with
    /// </summary>
    public class MatchmakerClient
    {
        internal static MatchmakerClient Instance = null!;

        /// <summary>
        /// Enabled when <see cref="Update"/> needs to be ran to update the client.
        /// </summary>
        public bool ShouldUpdate { get; private set; }

        internal readonly RiptideClient Client;
        internal readonly IClientHandler Handler;

        private readonly byte rankId;
        private readonly ulong playerId;

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="ip">Target ip of the matchmaker server</param>
        /// <param name="clientHandler">Handler for the client</param>
        /// <param name="rankId">Id of the clients rank</param>
        /// <param name="playerId">Player id (SteamId for example)</param>
        public MatchmakerClient(string ip, IClientHandler clientHandler, byte rankId, ulong playerId)
        {
            // init logs
            CopperLogger.Initialize(CopperLogger.InternalLogInfo, CopperLogger.InternalLogWarning, CopperLogger.InternalLogError);
            RiptideLogger.Initialize(CopperLogger.LogInfo, CopperLogger.LogInfo, CopperLogger.LogWarning, CopperLogger.LogError, false);

            // values/handlers
            this.rankId = rankId;
            this.playerId = playerId;
            Handler = clientHandler;
            Instance = this;

            // start riptide crap
            Client = new RiptideClient(new TcpClient());
            ShouldUpdate = true;
            
            Client.Connect(ip, 5, 0, null, false);
            Client.Connection.CanQualityDisconnect = false;
            
            Client.Connected += ClientConnectedHandler;
            Client.MessageReceived += ClientMessageHandlers.ClientReceivedMessageHandler;
            Client.Disconnected += ClientDisconnectedHandler;
        }

        ~MatchmakerClient()
        {
            ShouldUpdate = false;
            Client.Connected -= ClientConnectedHandler;
            Client.MessageReceived -= ClientMessageHandlers.ClientReceivedMessageHandler;
            Client.Disconnected -= ClientDisconnectedHandler;
        }

        /// <summary>
        /// Method to run often to update the client
        /// </summary>
        public void Update()
        {
            if (ShouldUpdate)
                Client.Update();
        }

        private void ClientConnectedHandler(object sender, EventArgs eventArgs)
        {
            var joinMessage = Message.Create(MessageSendMode.Reliable, MessageIds.ClientJoined);

            joinMessage.Add(playerId); // ulong
            joinMessage.Add(rankId); // byte

            Log.Info($"Creating client join message. | PlayerId {playerId} | RankId {rankId}");
            Client.Send(joinMessage);
        }
        
        private void ClientDisconnectedHandler(object sender, DisconnectedEventArgs args)
        {
            Log.Info($"Client disconnected | Reason: {args.Reason}");
            Handler.Disconnected(args.Reason);
        }
    }
}
