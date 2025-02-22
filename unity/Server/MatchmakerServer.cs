using System.Collections.Generic;
using System.Linq;
using CopperMatchmaking.Data;
using CopperMatchmaking.Info;
using CopperMatchmaking.Util;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;
using RiptideServer = Riptide.Server;

namespace CopperMatchmaking.Server
{
    /// <summary>
    /// 
    /// </summary>
    public class MatchmakerServer : Singleton<MatchmakerServer>
    {
        internal readonly RiptideServer Server = null!;

        internal readonly List<Rank> Ranks = new List<Rank>();

        internal readonly ServerQueueManager QueueManager = null!;
        internal readonly ServerLobbyManager LobbyManager = null!;
        internal readonly ServerHandler Handler;

        /// <summary>
        /// Time in seconds that the host of a lobby has to send the join code for said lobby 
        /// </summary>
        public float LobbyTimeoutTime = 5;
        
        /// <summary>
        /// Base Constructor with a pre-made <see cref="ServerHandler"/>
        /// </summary>
        /// <param name="lobbySize">Size of a lobby. Must be an even number</param>
        /// <param name="maxClients">Max amount of clients that can connect to the matchmaking server</param>
        public MatchmakerServer(byte lobbySize = 10, ushort maxClients = 65534) : this(new ServerHandler(), lobbySize, maxClients)
        {
        }

        /// <summary>
        /// Base Constructor
        /// </summary>
        /// <param name="handler"><see cref="ServerHandler"/></param>
        /// <param name="lobbySize">Size of a lobby. Must be an even number</param>
        /// <param name="maxClients">Max amount of clients that can connect to the matchmaking server</param>
        public MatchmakerServer(ServerHandler handler, byte lobbySize = 10, ushort maxClients = 65534)
        {
            // values
            this.Handler = handler;
            SetInstance(this);

            // checks
            if (lobbySize % 2 != 0)
            {
                Log.Error($"Lobby size is not divisible by 2.");
                return;
            }

            // logs
            CopperLogger.Initialize(CopperLogger.InternalLogInfo, CopperLogger.InternalLogWarning, CopperLogger.InternalLogError);
            RiptideLogger.Initialize(CopperLogger.LogInfo, CopperLogger.LogInfo, CopperLogger.LogWarning, CopperLogger.LogError, false);

            // networking
            Server = new RiptideServer(new TcpServer());
            Server.Start(7777, maxClients, 0, false);

            // matchmaking 
            QueueManager = new ServerQueueManager(lobbySize);
            LobbyManager = new ServerLobbyManager(this);

            // actions
            QueueManager.PotentialLobbyFound += LobbyManager.PotentialLobbyFound;
            Server.ClientDisconnected += QueueManager.ClientDisconnected;
            Server.MessageReceived += ServerMessageHandlers.ServerReceivedMessageHandler;
        }
        
        ~MatchmakerServer()
        {
            QueueManager.PotentialLobbyFound -= LobbyManager.PotentialLobbyFound;
            Server.ClientDisconnected -= QueueManager.ClientDisconnected;
            Server.MessageReceived -= ServerMessageHandlers.ServerReceivedMessageHandler;
                
            SetInstance(null);
        }

        /// <summary>
        /// Method to run often to update the server
        /// </summary>
        public void Update()
        {
            // internal crap
            LobbyManager.TimeoutCheck();
            QueueManager.CheckForLobbies();

            // networking
            Server.Update();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetRanks">Ranks to register</param>
        public void RegisterRanks(params Rank[] targetRanks)
        {
            // bytes are used internally for rank ids so we dont want more than the max amount of a byte
            if (Ranks.Count + targetRanks.Length > byte.MaxValue - 1)
                return;

            Ranks.AddRange(targetRanks.ToList());
            Log.Info(
                $"Registering {targetRanks.Length} new ranks, bringing the total to {Ranks.Count}. | Ranks: {Ranks.Aggregate("", (current, rank) => current + $"{rank.DisplayName}[{rank.Id}], ")}");

            QueueManager.RegisterRanks(Ranks);
        }

        internal void RegisterClient(ConnectedClient client)
        {
            Log.Info($"New Client Joined | Rank: {client.Rank.DisplayName} | ConnectionId: {client.ConnectionId}");

            if (!Handler.VerifyPlayer(client))
            {
                Log.Info($"Couldn't verify client. Disconnecting");
                return;
            }

            QueueManager.RegisterPlayer(client);
        }

        internal void SendMessage(Message message, ushort toClient, bool shouldRelease = true) => Server.Send(message, toClient, shouldRelease);

        internal ushort SendMessage(Message message, Connection toClient, bool shouldRelease = true) => Server.Send(message, toClient, shouldRelease);

        internal void SendMessageToAll(Message message, bool shouldRelease = true) => Server.SendToAll(message, shouldRelease);

        internal void SendMessageToAll(Message message, ushort exceptToClientId, bool shouldRelease = true) => Server.SendToAll(message, exceptToClientId, shouldRelease);
    }
}