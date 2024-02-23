using CopperMatchmaking.Data;
using CopperMatchmaking.Server;

namespace CopperMatchmaking.Example.Server;

public static class Program
{
    public static void Main()
    {
        byte lobbySize = byte.Parse(Environment.GetEnvironmentVariable("LOBBY_SIZE") ?? "4");

        string[] rankNames = (Environment.GetEnvironmentVariable("RANKS") ?? "Unranked,Bronze,Silver,Gold,Platinum,Diamond,Master,Chaos").Split(',');
        var ranks = new Rank[rankNames.Length];
        for (int i = 0; i < rankNames.Length; i++)
        {
            ranks[i] = new Rank(rankNames[i], (byte)i);
        }

        var server = new MatchmakerServer(lobbySize);
        server.RegisterRanks(ranks);

        while (true)
        {
            server.Update();
        }
    }
}
