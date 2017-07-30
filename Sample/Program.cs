using LeagueReplayParser;
using System;
using System.Text;

namespace Sample
{
    public class Program
    {
        static void Main(string[] args) => new Program();

        public Program()
        {
            this.LoadAndDisplay();
            Console.Read();
        }

        private async void LoadAndDisplay()
        {
            try
            {
                League league = new League(@"D:\Jeux\Riot");
                await league.LoadReplaysAsync(Encoding.UTF8);

                Console.WriteLine($"League version: {league.Version}");
                Console.WriteLine($"Found {league.Replays.Count} replays.");

                if (league.Replays.Count > 0)
                {
                    Replay replay = league.Replays[0];
                    Console.WriteLine($"Looking out first replay.");
                    Console.WriteLine($"League version: {replay.GameVersion.ToString()}");
                    Console.WriteLine($"Game duration: {replay.GameLength.ToString()}");
                    Console.WriteLine($"Winning team: {replay.WiningTeam.Side.ToString()}");

                    foreach (Player player in replay.WiningTeam.Players)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"+ {player.PlayerName} as {player.ChampionName}:");
                        Console.WriteLine($"   Level: {player.Level}");
                        Console.WriteLine($"   KDA: {player.KDA.Kills}/{player.KDA.Deaths}/{player.KDA.Assistances} ({player.KDA.Ratio})");
                        Console.WriteLine($"   Lane: {player.Lane}");
                        Console.WriteLine($"   Minion score: {player.MinionScore}");
                    }

                }
            }
            catch (Exception ex)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ForegroundColor = color;
            }
            
        }
    }
}
