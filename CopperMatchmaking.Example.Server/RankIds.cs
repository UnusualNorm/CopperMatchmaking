using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopperMatchmaking.Example.Server;

public enum RankIds : byte
{
    Unranked = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4,
    Diamond = 5,
    Master = 6,
    Chaos = 7
}