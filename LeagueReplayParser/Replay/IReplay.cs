using System.IO;

namespace LeagueReplayParser
{
#pragma warning disable CS1591

    public abstract class IReplay
    {
        /// <summary>
        /// Path to the replay file.
        /// </summary>
        public FileInfo Path { get; private set; }

        public IReplay(string replayPath)
        {
            FileInfo replay = new FileInfo(replayPath);

            if (!replay.Exists || replay.Extension != ".rofl")
                throw new IOException("The given path is not a valid replay file.");

            this.Path = replay;
        }
    }

#pragma warning restore CS1591
}
