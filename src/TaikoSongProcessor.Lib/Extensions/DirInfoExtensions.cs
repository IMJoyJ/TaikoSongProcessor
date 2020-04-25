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

        public static FileInfo GetMusicFile(this DirectoryInfo directory)
        {
            return directory.GetFiles("main.ogg").Any() ? directory.GetFiles("main.ogg").FirstOrDefault() : directory.GetFiles("*.ogg").FirstOrDefault();
        }

        public static FileInfo[] GetOszFiles(this DirectoryInfo directory)
        {
            return directory.GetFiles("*.osz");
        }

        public static bool ContainsSong(this DirectoryInfo directory)
        {
            FileInfo tjaFile = directory.GetTjaFile();

            FileInfo musicFile = directory.GetMusicFile();

            FileInfo[] oszFiles = directory.GetOszFiles();

            return (tjaFile != null && musicFile != null) || oszFiles.Any();
        }
    }
}
