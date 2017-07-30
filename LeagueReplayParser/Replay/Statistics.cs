namespace LeagueReplayParser
{

    /// <summary>
    /// Static statistic class for replay files.
    /// </summary>
    public static class Statistics
    {

        #pragma warning disable CS1591
        #region Classes

        /// <summary>
        /// Player's KDA class.
        /// </summary>
        public class KDA
        {
            /// <summary>
            /// Total champion killed.
            /// </summary>
            public int Kills { get; set; }

            /// <summary>
            /// Total deaths.
            /// </summary>
            public int Deaths { get; set; }

            /// <summary>
            /// Total assistances.
            /// </summary>
            public int Assistances { get; set; }

            /// <summary>
            /// Gets the KDA for these statistics.
            /// </summary>
            /// <returns></returns>
            public float Ratio
            {
                get
                {
                    return (Kills + Assistances) / Deaths;
                }
            }
        }

        /// <summary>
        /// Player's inventory.
        /// Those are the IDs of the items. To get the real items, use the static riot API (RiotSharp for instance).
        /// </summary>
        public class Inventory
        {
            public int Item1 { get; set; }
            public int Item2 { get; set; }
            public int Item3 { get; set; }
            public int Item4 { get; set; }
            public int Item5 { get; set; }
            public int Item6 { get; set; }
            public int Trinket { get; set; }
        }

        #endregion
        
        #region Enums

        /// <summary>
        /// The lane the player was playing on. 
        /// </summary>
        public enum Lane
        {
            Support = 0,
            Top = 1,
            Mid = 2,
            Jungle = 3,
            Bot = 4
        }

        /// <summary>
        /// The issue of the game for a team.
        /// </summary>
        public enum Issue
        {
            Victory,
            Defeat
        }

        /// <summary>
        /// The side of the map the team was on.
        /// </summary>
        public enum Side
        {
            Purple = 100,
            Blue = 200
        }

        #endregion
        #pragma warning restore CS1591

    }
}
