using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TaikoSongProcessor.Lib;
using TaikoSongProcessor.Lib.Models;

namespace TaikoSongProcessor.lib
{
    public class TjaProcessor
    {
        private readonly int categoryId;
        private int id;
        private List<string> tjaFileContents;

        public Song Process(FileInfo tjaFile, int id)
        {
            Song song = null;
            this.id = id;
            try
            {
                string fileContent = File.ReadAllText(tjaFile.FullName, Encoding.GetEncoding(932));
                this.tjaFileContents = File.ReadAllLines(tjaFile.FullName, Encoding.GetEncoding(932)).ToList();

                if (this.tjaFileContents.Count > 0)
                {
                    song = this.TjaToSong();
                    song.Hash = ToBase64String(this.GetMd5(fileContent), false, false).TrimEnd('=');
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Empty file!\n");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return song;
        }
        public static string ToBase64String(string str, bool convertSlash, bool onlyAllowAlphabetAndNumber)
        {
            byte[] resultArray = Encoding.UTF8.GetBytes(str);
            string result = Convert.ToBase64String(resultArray, 0, resultArray.Length);
            if (convertSlash)
            {
                result = result.Replace("/", "_").Replace("+", "-");
            }
            else if (onlyAllowAlphabetAndNumber)
            {
                result = result.Replace("/", "").Replace("+", "");
            }
            return result;
        }
        private string GetMd5(string password, int bit = 32)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder tmp = new StringBuilder();
            foreach (byte i in hashedDataBytes)
            {
                tmp.Append(i.ToString("x2"));
            }
            if (bit == 16)
            {
                return tmp.ToString().Substring(8, 16);
            }
            else if (bit == 32)
            {
                return tmp.ToString();//默认情况
            }
            else
            {
                return string.Empty;
            }
        }
        public TjaProcessor(int categoryId)
        {
            this.categoryId = categoryId;
        }

        /// <summary>
        /// Generate a <see cref="Song"/> object based on a list of strings produced from a tja file
        /// </summary>
        private Song TjaToSong()
        {
            var title = this.GetStringValue("title").Replace("feat", "ft");

            Song song = new Song
            {
                Id = id,
                CategoryId = categoryId,
                Title = this.GetStringValue("title").Replace("feat", "ft"),
                Subtitle = this.GetStringValue("subtitle"),
                Order = id,
                Preview = this.GetDoubleValue("demostart"),
                Type = SongTypeEnum.Tja.ToString().ToLower(),
                // Offset = GetDoubleValue("offset"),
                Offset = 0,
                TitleLang = this.GetLanguageStrings("title"),
                SubtitleLang = this.GetLanguageStrings("subtitle"),
                Courses = this.GetCourses()
                
            };

            double volume = this.GetDoubleValue("songvol");
            if (volume == 0)
            {
                song.Volume = 1;
            }
            else
            {
                song.Volume = volume / 100;
            }

            return song;
        }

        private string GetStringValue(string fieldName, List<string> list = null)
        {
            List<string> tjaList = list ?? this.tjaFileContents;

            string value = tjaList.FirstOrDefault(line => line.StartsWith(fieldName, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value?.Substring(value.IndexOf(':') + 1);
                if (value.StartsWith("--"))
                {
                    value = value.Remove(0, 2);
                }

                if (value.StartsWith("++"))
                {
                    value = value.Remove(0, 2);
                }
            }
            return value;
        }

        private double GetDoubleValue(string fieldName, List<string> list = null)
        {
            string value = this.GetStringValue(fieldName, list);

            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }

            return 0;
        }

        private LanguageStrings GetLanguageStrings(string fieldName)
        {
            LanguageStrings strings = new LanguageStrings
            {
                en = this.GetStringValue($"{fieldName}en"),
                cn = this.GetStringValue($"{fieldName}cn"),
                tw = this.GetStringValue($"{fieldName}tw"),
                ja = this.GetStringValue($"{fieldName}ja"),
                ko = this.GetStringValue($"{fieldName}ko")
            };

            return strings;
        }

        private Courses GetCourses()
        {
            Courses courses = new Courses()
            {
                Easy = this.GetCourse(DifficultyEnum.Easy),
                Normal = this.GetCourse(DifficultyEnum.Normal),
                Hard = this.GetCourse(DifficultyEnum.Hard),
                Oni = this.GetCourse(DifficultyEnum.Oni),
                Ura = this.GetCourse(DifficultyEnum.Ura)
            };

            return courses;
        }

        private Course GetCourse(DifficultyEnum difficulty)
        {

            List<string> findCourse = this.tjaFileContents.SkipWhile(line =>
                    !line.StartsWith($"course:{difficulty.ToString()}", StringComparison.InvariantCultureIgnoreCase) &&
                    !line.StartsWith($"course:{(int)difficulty}", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (findCourse.Count == 0)
            {
                return null;
            }

            Course course = new Course
            {
                Stars = (int) this.GetDoubleValue("level", findCourse)
            };

            return course;
        }
    }
}
