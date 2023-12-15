using System;
using System.Text;
using CopperMatchmaking.Info;
using CopperMatchmaking.Telepathy;

namespace CopperMatchmaking
{
    public class MatchmakerClient
    {
        public readonly byte RankId;

        internal Client Client;

        public MatchmakerClient(Enum id) : this(Convert.ToByte(id))
        {
        }

        public MatchmakerClient(byte rankId)
        {
            RankId = rankId;
            Client = new Client(Matchmaker.MaxMessageSize)
            {
                OnConnected = () =>
                {
                    Log.Info("Client Connected");
                },
                OnData = message =>
                {
                    // Log.Info($"Received new data from server. Raw Data: {BitConverter.ToString(message.Array, message.Offset, message.Count)}");
                    var receivedMessage = new Message(message);
                    Log.Info($"Received new data from server. Raw Data: {BitConverter.ToString(message.Array, message.Offset, message.Count)} | Message[{receivedMessage.Id}] receive is of type {((Message.MessageType)receivedMessage.Type).ToString()}. Data: {receivedMessage.GetData()}");
                },
                OnDisconnected = () =>
                {
                    Log.Info("Client Disconnected");
                }
            };
        }

        public void Connect(string ip, int port)
        {
            Client.Connect(ip, port);

            Client.OnConnected += () =>
            {
                const string welcomeMessage = "hello world";
                
                Log.Info($"Sending welcome message '{welcomeMessage}'");
                var message = new Message(0, welcomeMessage);
                Client.Send(message);
            };
        }

        public void Update()
        {
            Client.Tick(100);
        }
    }
}