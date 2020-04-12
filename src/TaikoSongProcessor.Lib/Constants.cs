using System.Collections.Generic;

namespace TaikoSongProcessor.Lib
{
    public enum SongTypeEnum
    {
        Tja,
        Osu
    }

    public enum DifficultyEnum
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Oni = 3,
        Ura = 4
    }

    public static class DifficultyLabels
    {
        public static string EasyRegex = @"^.*\[.*(ea(s|z)y|kantan|簡単).*\].*$";
        public static string NormalRegex = @"^.*\[.*(normal|futsu{1,2}|普通).*\].*$";
        public static string HardRegex = @"^.*\[.*(hard|muzukashii|難しい).*\].*$";
        public static string OniRegex = @"^.*\[.*((?<!inner\s)oni|extreme|marathon).*\].*$";
        public static string UraRegex = @"^.*\[.*((inner oni)|ura|insane).*\].*$";
    }

}
