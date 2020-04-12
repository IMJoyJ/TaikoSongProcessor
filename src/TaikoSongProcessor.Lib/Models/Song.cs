using System;
using System.Text.Json.Serialization;

namespace TaikoSongProcessor.Lib.Models
{
    public class Song
    {
        /// <summary>
        /// ID used in Taiko-web;
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title_lang")]
        public LanguageStrings TitleLang { get; set; } = new LanguageStrings();

        [JsonPropertyName("subtitle_lang")]
        public LanguageStrings SubtitleLang { get; set; } = new LanguageStrings();

        [JsonPropertyName("courses")]
        public Courses Courses { get; set; } = new Courses();

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = true; //not really doing anything with this (yet) so we'll just enable everything by default I guess

        /// <summary>
        /// Generic title of the song
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// Generic subtitle of the song
        /// </summary>
        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }

        /// <summary>
        /// Category Id used by Taiko-Web
        /// </summary>
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Type of song.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("offset")]
        public double Offset { get; set; }

        /// <summary>
        /// ID of skin to use while playing song. Not yet supported.
        /// </summary>
        [JsonPropertyName("skin_id")]
        public int SkinId { get; set; }

        /// <summary>
        /// Offset of song preview during song selection in seconds, defaults to zero.
        /// </summary>
        [JsonPropertyName("preview")]
        public double Preview { get; set; } = 0;

        /// <summary>
        /// Volume of song, defaults to 1
        /// </summary>
        [JsonPropertyName("volume")]
        public double Volume { get; set; } = 1;

        /// <summary>
        /// Maker ID. Not yet supported.
        /// </summary>
        [JsonPropertyName("maker_id")]
        public int MakerId { get; set; }

        /// <summary>
        /// Show lyrics during play. Not yet supported.
        /// </summary>
        [JsonPropertyName("lyrics")]
        public bool Lyrics { get; set; } = false;

        /// <summary>
        /// Unique hash used for highscores and stuff
        /// </summary>
        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        /// <summary>
        /// Order of song in song selection screen. We're using the ID for this at the moment.
        /// </summary>
        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("generatedOn")]
        public DateTime GeneratedOn { get; set; }

        public Song()
        {
            GeneratedOn = DateTime.Now;
            Hash = Guid.NewGuid()
                .ToString(); //Taiko-web doesn't actually require a real hash here - any randomized poo will work. Used to id highscores.
        }
    }

    /// <summary>
    /// Strings to use for various languages. Used for titles and subtitles.
    /// </summary>
    public class LanguageStrings
    {
        public string ja { get; set; }
        public string en { get; set; }
        public string cn { get; set; }
        public string tw { get; set; }
        public string ko { get; set; }
    }

    public class Courses
    {
        [JsonPropertyName("easy")]
        public Course Easy { get; set; }
        [JsonPropertyName("normal")]
        public Course Normal { get; set; }
        [JsonPropertyName("hard")]
        public Course Hard { get; set; }
        [JsonPropertyName("oni")]
        public Course Oni { get; set; }
        [JsonPropertyName("ura")]
        public Course Ura { get; set; }

        public bool HasCourses()
        {
            return Easy != null || Normal != null || Hard != null || Oni != null || Ura != null;
        }
    }

    public class Course
    {
        [JsonPropertyName("stars")]
        public int Stars { get; set; }
        [JsonPropertyName("branch")]
        public bool Branch { get; set; }
    }
}
