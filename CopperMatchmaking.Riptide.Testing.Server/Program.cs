﻿using CopperMatchmaking;
using CopperMatchmaking.Riptide.Testing.Server;

var server = new MatchmakerServer();
server.RegisterRanks(
    new Rank("Unranked", RankIds.Unranked), 
    new Rank("Bronze", RankIds.Bronze),
    new Rank("Silver", RankIds.Silver), 
    new Rank("Gold", RankIds.Gold),
    new Rank("Platinum", RankIds.Platinum),
    new Rank("Diamond", RankIds.Diamond),
    new Rank("Master", RankIds.Master),
    new Rank("Chaos", RankIds.Chaos));

while (true)
{
    server.Update();
}