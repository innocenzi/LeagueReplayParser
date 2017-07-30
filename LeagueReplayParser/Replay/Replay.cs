using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueReplayParser.Statistics;
using System.IO;

namespace LeagueReplayParser
{

    /// <summary>
    /// Replay class representing replay files.
    /// </summary>
    #pragma warning disable CS1591
    public class Replay : IReplay
    {

        /// <summary>
        /// Defines weither yes or not this replay can be played. Returns null if no League object has been initialized and thus the League version is unknown.
        /// </summary>
        public bool? CanBePlayed
        {
            get
            {
                if (League.LeagueVersion == null)
                    return null;
                return !(this.GameVersion < League.LeagueVersion);
            }
        }
        public TimeSpan GameLength { get; set; }
        public Version GameVersion { get; set; }
        public Team PurpleTeam { get; set; }
        public Team BlueTeam { get; set; }
        public List<Player> Players { get; set; }
        public Team WiningTeam
        {
            get
            {
                return (PurpleTeam.Issue == Issue.Victory ? PurpleTeam : BlueTeam);
            }
        }
        public Team LosingTeam
        {
            get
            {
                return (PurpleTeam.Issue == Issue.Victory ? PurpleTeam : BlueTeam);
            }
        }

        // See IReplay constructor
        internal Replay(string replayPath) : base(replayPath) { }

        /// <summary>
        /// Aynchronously returns a Replay instance given the replay file.
        /// </summary>
        public async static Task<Replay> ParseAsync(string replayPath, Encoding encoding = null)
        {
            return await new Parser(replayPath).GetReplayAsync(encoding ?? Encoding.Default);
        }

        /// <summary>
        /// Returns a Replay instance given the replay file.
        /// </summary>
        public static Replay Parse(string replayPath, Encoding encoding = null)
        {
            return new Parser(replayPath).GetReplay(encoding ?? Encoding.Default);
        }


    }

    /// <summary>
    /// Helper class for JSON parsing.
    /// </summary>
    public class Parser : IReplay
    {

        // See IReplay constructor
        public Parser(string replayPath) : base(replayPath) { }

        /// <summary>
        /// Aynchronously gets the replay instance corresponding to the given replay file.
        /// </summary>
        public async Task<Replay> GetReplayAsync(Encoding encoding)
        {
            return await Task<Replay>.Run(() =>
            {
                return this.GetReplay(encoding ?? Encoding.Default);
            });

        }

        /// <summary>
        /// Gets the replay instance corresponding to the given replay file.
        /// </summary>
        public Replay GetReplay(Encoding encoding)
        {
            Replay replay = new Replay(this.Path.FullName);

            string replayFileContents = string.Join("", File.ReadLines(this.Path.FullName, encoding ?? Encoding.Default).Take(20).ToList<string>().ToArray<string>());
            JObject json = this.GetJSON(replayFileContents);
            List<Player> players = this.GetPlayers(JArray.Parse(json["statsJson"].ToObject<string>()));

            replay.GameLength = TimeSpan.FromMilliseconds(json["gameLength"].ToObject<float>());
            replay.GameVersion = new Version(json["gameVersion"].ToObject<string>());
            replay.PurpleTeam = this.GetTeam(Side.Purple, players);
            replay.BlueTeam = this.GetTeam(Side.Blue, players);
            replay.Players = players;

            return replay;
        }

        /// <summary>
        /// Gets the JSON part of the replay file contents.
        /// </summary>
        public JObject GetJSON(string replayFileContents)
        {
            int jsonStartIndex = replayFileContents.IndexOf("{\"gameLength\"");
            int jsonEndIndex = replayFileContents.IndexOf("\\\"}]\"}") + "\\\"}]\"}".Length;

            try
            {
                return JObject.Parse(replayFileContents.Substring(jsonStartIndex, (jsonEndIndex - jsonStartIndex)));
            }
            catch (JsonReaderException jre)
            {
                throw new Exception("Unable to parse replay data from this replay file.", jre);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error has occured while trying to parse replay data.", ex);
            }
        }

        /// <summary>
        /// Gets the purple team object thanks to the json array
        /// </summary>
        public List<Player> GetPlayers(JArray jsonTeams)
        {
            List<Player> players = new List<Player>() { };

            // loops through json array
            foreach (JObject jsonPlayer in jsonTeams)
            {
                Player player = new Player()
                {
                    PlayerName = jsonPlayer["NAME"].ToObject<string>(),
                    PlayerId = jsonPlayer["ID"].ToObject<long>(),
                    ChampionName = jsonPlayer["SKIN"].ToObject<string>(),
                    Level = jsonPlayer["LEVEL"].ToObject<int>(),
                    Team = (Side)jsonPlayer["TEAM"].ToObject<int>(),
                    Win = jsonPlayer["WIN"].ToObject<string>(),
                    KeystoneID = jsonPlayer["KEYSTONE_ID"].ToObject<int>(),
                    KDA = new KDA()
                    {
                        Kills = jsonPlayer["CHAMPIONS_KILLED"].ToObject<int>(),
                        Assistances = jsonPlayer["ASSISTS"].ToObject<int>(),
                        Deaths = jsonPlayer["NUM_DEATHS"].ToObject<int>(),
                    },
                    Lane = (Lane)jsonPlayer["PLAYER_POSITION"].ToObject<int>(),
                    MinionScore = jsonPlayer["MINIONS_KILLED"].ToObject<int>() +
                                  jsonPlayer["NEUTRAL_MINIONS_KILLED"].ToObject<int>() +
                                  jsonPlayer["NEUTRAL_MINIONS_KILLED_YOUR_JUNGLE"].ToObject<int>() +
                                  jsonPlayer["NEUTRAL_MINIONS_KILLED_ENEMY_JUNGLE"].ToObject<int>(),
                    Inventory = new Inventory()
                    {
                        Item1 = jsonPlayer["ITEM0"].ToObject<int>(),
                        Item2 = jsonPlayer["ITEM1"].ToObject<int>(),
                        Item3 = jsonPlayer["ITEM2"].ToObject<int>(),
                        Item4 = jsonPlayer["ITEM4"].ToObject<int>(),
                        Item5 = jsonPlayer["ITEM5"].ToObject<int>(),
                        Item6 = jsonPlayer["ITEM6"].ToObject<int>(),
                        Trinket = jsonPlayer["ITEM3"].ToObject<int>(),
                    },
                    Summoner1 = jsonPlayer["SUMMON_SPELL1_CAST"].ToObject<int>(),
                    Summoner2 = jsonPlayer["SUMMON_SPELL2_CAST"].ToObject<int>(),
                };
                players.Add(player);
            }

            return players;
        }

        /// <summary>
        /// Gets the players in the given team side.
        /// </summary>
        public Team GetTeam(Side side, List<Player> players)
        {
            return new Team()
            {
                Side = side,
                Players = players.FindAll(x => x.Team == side),
                Issue = players.Find(x => x.Team == side).Issue
            };
        }

    }

    #pragma warning restore CS1591
}
