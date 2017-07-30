using System.Collections.Generic;
using static LeagueReplayParser.Statistics;

namespace LeagueReplayParser
{

    #pragma warning disable CS1591

    public class Team
    {
        public Issue Issue { get; set; }
        public Side Side { get; set; }
        public List<Player> Players { get; set; }
    }

    #pragma warning restore CS1591

}
