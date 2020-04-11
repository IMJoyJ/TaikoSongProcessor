using System.IO;
using System.Linq;

namespace TaikoSongProcessor.Lib.Extensions
{
    public static class DirInfoExtensions
    {
        public static FileInfo GetTjaFile(this DirectoryInfo directory)
        {
            return directory.GetFiles("main.tja").Any() ? directory.GetFiles("main.tja").First() : directory.GetFiles("*.tja").First();
        }

        public static FileInfo GetMp3File(this DirectoryInfo directory)
        {
            return directory.GetFiles("main.mp3").Any() ? directory.GetFiles("main.mp3").First() : directory.GetFiles("*.mp3").First();
        }

        public static bool ContainsSong(this DirectoryInfo directory)
        {
            FileInfo tjaFile = directory.GetTjaFile();

            FileInfo mp3File = directory.GetMp3File();

            return tjaFile != null && mp3File != null;
        }
    }
}
