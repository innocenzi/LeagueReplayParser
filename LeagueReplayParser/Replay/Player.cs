using static LeagueReplayParser.Statistics;

namespace LeagueReplayParser
{

#pragma warning disable CS1591

    public class Player
    {
        public long PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string ChampionName { get; set; }

        public string Win { get; set; }
        public Issue Issue
        {
            get
            {
                return (Win == "Win" ? Issue.Victory : Issue.Defeat);
            }
        }

        public int Level { get; set; }
        public int MinionScore { get; set; }
        public int KeystoneID { get; set; }

        /// <summary>
        /// Use Static Riot API to determine which summoner spell this ID corresponds to.
        /// </summary>
        public int Summoner1 { get; set; }

        /// <summary>
        /// Use Static Riot API to determine which summoner spell this ID corresponds to.
        /// </summary>
        public int Summoner2 { get; set; }

        public Side Team { get; set; }
        public Lane Lane { get; set; }
        public KDA KDA { get; set; }
        public Inventory Inventory { get; set; }

    }

#pragma warning restore CS1591



}
