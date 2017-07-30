using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LeagueReplayParser
{
    
    /// <summary>
    /// Contains all the data form League of Legends (its path and its replays' path).
    /// You will be able to start a replay thanks to this class.
    /// </summary>
    public class League
    {
        
        /// <summary>
        /// The default Riot directory.
        /// </summary>
        public static readonly string DefaultRiotDirectory = Path.GetFullPath(@"C:\Riot Games");

        /// <summary>
        /// The default Replays' directory.
        /// </summary>
        public static readonly string DefaultReplaysDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "League of Legends", "Replays");

        #pragma warning disable CS1591

        public DirectoryInfo RiotDirectory { get; private set; }
        public DirectoryInfo ReplayDirectory { get; private set; }
        public Version Version { get; private set; }
        public Replay CurrentReplay { get; private set; }

        #pragma warning restore CS1591

        /// <summary>
        /// Creates an instance of a League object which contains the Riot and replays directories
        /// </summary>
        /// <param name="riotDirectory"></param>
        /// <param name="replayDirectory"></param>
        public League(string riotDirectory, string replayDirectory = null)
        {
            this.RiotDirectory = this.GetRiotDirectory(riotDirectory);
            this.ReplayDirectory = this.GetReplayDirectory(replayDirectory ?? League.DefaultReplaysDirectory);
            this.Version = this.GetCurrentLeagueVersion();
        }

        #region DataGathering

        /// <summary>
        /// Gets the current League of Legends version.
        /// </summary>
        public Version GetCurrentLeagueVersion()
        {
            if (!this.RiotDirectory.Exists)
                throw new IOException("Given Riot directory does not exists.");

            DirectoryInfo releases = new DirectoryInfo(Path.Combine(this.RiotDirectory.FullName, "RADS", "solutions", "lol_game_client_sln", "releases"));

            if (!releases.Exists)
                throw new IOException($"Unable to find client directory {releases.FullName}. Please verify your installation.");

            foreach (DirectoryInfo release in releases.GetDirectories("*.*.*.*", SearchOption.TopDirectoryOnly))
                return new Version(FileVersionInfo.GetVersionInfo(Path.Combine(release.FullName, "deploy", "League of Legends.exe")).ProductVersion);

            throw new IOException($"Unable to find client directory in {releases.FullName}. Please erify your installation.");
        }

        /// <summary>
        /// Verifies if the given riot directory contains the right files.
        /// </summary>
        private DirectoryInfo GetRiotDirectory(string givenRiotDirectory)
        {
            if (new FileInfo(Path.Combine(givenRiotDirectory, "lol.launcher.exe")).Exists)
                return new DirectoryInfo(givenRiotDirectory);

            if (new FileInfo(Path.Combine(League.DefaultRiotDirectory, "lol.launcher.exe")).Exists)
                return new DirectoryInfo(League.DefaultRiotDirectory);

            throw new IOException("The given Riot directory does not contains the correct files. Please verify the path.");
        }

        /// <summary>
        /// Verifies if the given replay directory exists.
        /// </summary>
        private DirectoryInfo GetReplayDirectory(string givenReplayDirectory)
        {
            if (!new DirectoryInfo(givenReplayDirectory).Exists)
                this.ReplayDirectory = new DirectoryInfo(League.DefaultReplaysDirectory);
            else
                this.ReplayDirectory = new DirectoryInfo(givenReplayDirectory);

            if (!this.ReplayDirectory.Exists)
                throw new IOException("The given replay directory does not exist.");

            //if (this.ReplayDirectory.GetFiles("*.rofl", SearchOption.TopDirectoryOnly).Count() == 0)
            // Warning: no replay file

            return this.ReplayDirectory;
        }

        /// <summary>
        /// Get the League of Legends process info.
        /// It is internally managed. You normally won't have to use it.
        /// </summary>
        /// <returns></returns>
        public ProcessStartInfo GetLeagueProcess(Replay replay)
        {
            if (!this.RiotDirectory.Exists)
                throw new IOException("Given Riot directory does not exists.");

            DirectoryInfo releases = new DirectoryInfo(Path.Combine(this.RiotDirectory.FullName, "RADS", "solutions", "lol_game_client_sln", "releases"));
            DirectoryInfo leagueExecutableDirectory = null;

            if (!releases.Exists)
                throw new IOException($"Unable to find client directory {releases.FullName}. Please verify your installation.");

            foreach (DirectoryInfo release in releases.GetDirectories("*.*.*.*", SearchOption.TopDirectoryOnly))
                leagueExecutableDirectory = new DirectoryInfo(Path.Combine(release.FullName, "deploy"));

            if (leagueExecutableDirectory == null || !leagueExecutableDirectory.Exists ||
                !File.Exists(Path.Combine(leagueExecutableDirectory.FullName, "League of Legends.exe")))
                throw new IOException($"Unable to find League of Legends executable file. Please verify your installation.");

            return new ProcessStartInfo()
            {
                UseShellExecute = true,
                WorkingDirectory = leagueExecutableDirectory.FullName,
                FileName = "League of Legends.exe",
                Verb = "open",
                Arguments = $"\"{replay.Path}\"",
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        #endregion


        /// <summary>
        /// Starts the given replay and returns true once the League of Legends process is closed.
        /// </summary>
        public async Task<bool> StartReplayAsync(Replay replay)
        {
            bool replayTask = await Task<bool>.Run(() =>
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

                Process league = new Process()
                {
                    StartInfo = this.GetLeagueProcess(replay),
                    EnableRaisingEvents = true
                };

                league.Exited += (sender, args) =>
                {
                    task.SetResult(true);
                    league.Dispose();
                };

                bool started = league.Start();
                if (!started)
                    throw new InvalidOperationException("Unable to start League of Legends.exe.");

                this.CurrentReplay = replay;

                return task.Task.Result;
            }).ConfigureAwait(false);

            this.CurrentReplay = null;
            return replayTask;
        }

    }
}
