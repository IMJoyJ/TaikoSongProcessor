using System.IO;
using System.Linq;

namespace TaikoSongProcessor.Lib.Extensions
{
    public static class DirInfoExtensions
    {
        public static FileInfo GetTjaFile(this DirectoryInfo directory)
        {
            return directory.GetFiles("main.tja").Any() ? directory.GetFiles("main.tja").FirstOrDefault() : directory.GetFiles("*.tja").FirstOrDefault(tja=>!tja.Name.Contains("裏")); //don't select backsides
        }

        public static FileInfo GetMp3File(this DirectoryInfo directory)
        {
            return directory.GetFiles("main.mp3").Any() ? directory.GetFiles("main.mp3").FirstOrDefault() : directory.GetFiles("*.mp3").FirstOrDefault();
        }

        public static FileInfo[] GetOszFiles(this DirectoryInfo directory)
        {
            return directory.GetFiles("*.osz");
        }

        public static bool ContainsSong(this DirectoryInfo directory)
        {
            FileInfo tjaFile = directory.GetTjaFile();

            FileInfo mp3File = directory.GetMp3File();

            FileInfo[] oszFiles = directory.GetOszFiles();

            return (tjaFile != null && mp3File != null) || oszFiles.Any();
        }
    }
}
