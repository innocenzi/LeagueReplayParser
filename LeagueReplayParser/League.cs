using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        /// The actual League version.
        /// </summary>
        public static Version LeagueVersion { get; private set; }
        
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
        public List<Replay> Replays { get; set; }
        public Replay CurrentReplay { get; private set; }
        public Version Version { get; private set; }

#pragma warning restore CS1591

        /// <summary>
        /// Creates an instance of a League object which contains the Riot and replays directories
        /// </summary>
        public League(string riotDirectory, string replayDirectory = null, bool loadReplays = false)
        {
            this.Replays = new List<Replay>() { };
            this.RiotDirectory = this.GetRiotDirectory(riotDirectory);
            this.Version = this.GetCurrentLeagueVersion();
            this.ReplayDirectory = this.GetReplayDirectory(replayDirectory ?? League.DefaultReplaysDirectory);

            League.LeagueVersion = this.Version;

            if (loadReplays)
                this.LoadReplays();
        }

        /// <summary>
        /// Starts the given replay and returns true once the League of Legends process is closed.
        /// </summary>
        public async Task<bool> StartReplayAsync(Replay replay)
        {
            return await this.StartReplayAsync(replay.Path);
        }

        /// <summary>
        /// Starts the given replay and returns true once the League of Legends process is closed.
        /// </summary>
        public async Task<bool> StartReplayAsync(FileInfo replay)
        {
            if (!replay.Exists)
                throw new IOException("The given replay file does not exist.");

            bool replayTask = await Task<bool>.Run(async () =>
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();

                Process league = new Process()
                {
                    StartInfo = this.GetLeagueProcess(replay.FullName),
                    EnableRaisingEvents = true
                };

                league.Exited += (sender, args) =>
                {
                    task.SetResult(true);
                    league.Dispose();
                };

                bool started = league.Start();
                if (!started)
                    throw new Exception("Unable to start League of Legends.exe.");

                this.CurrentReplay = await Replay.ParseAsync(replay.FullName);

                return task.Task.Result;
            }).ConfigureAwait(false);

            this.CurrentReplay = null;
            return replayTask;
        }


        /// <summary>
        /// Asynchronously loads all the replays in the replay folder.
        /// </summary>
        public async Task LoadReplaysAsync(Encoding encoding = null)
        {
            this.Replays = await this.GetReplaysAsync(encoding ?? Encoding.Default);
        }

        /// <summary>
        /// Loads all the replays in the replay folder.
        /// Alias for LoadReplaysAsync.Result.
        /// </summary>
        public void LoadReplays()
        {
            this.Replays = this.GetReplaysAsync().Result;
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
        /// Aynchronously gets all the available replays.
        /// </summary>
        /// <exception cref="IOException">Thrown if the ReplayDirectory is not set yet.</exception>
        private async Task<List<Replay>> GetReplaysAsync(Encoding encoding = null)
        {
            if (this.ReplayDirectory == null || !this.ReplayDirectory.Exists)
                throw new IOException("Replay directory does not exists.");

            return await Task<List<Replay>>.Run(async () =>
            {
                List<Replay> replays = new List<Replay>() { };

                foreach (FileInfo replayFile in this.ReplayDirectory.GetFiles("*.rofl"))
                    replays.Add(await Replay.ParseAsync(replayFile.FullName, encoding ?? Encoding.Default));

                return replays;
            });
        }

        /// <summary>
        /// Get the League of Legends process info.
        /// It is internally managed. You normally won't have to use it.
        /// </summary>
        /// <returns></returns>
        public ProcessStartInfo GetLeagueProcess(string replayPath)
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
                Arguments = $"\"{replayPath}\"",
                WindowStyle = ProcessWindowStyle.Hidden
            };
        }

        #endregion


    }
}
