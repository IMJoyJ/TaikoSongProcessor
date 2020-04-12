using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TaikoSongProcessor.Lib;
using TaikoSongProcessor.Lib.Models;

namespace TaikoSongProcessor.lib
{
    public class TjaProcessor
    {
        private readonly int _categoryId;
        private int _id;
        private List<string> _tjaFileContents;

        public Song Process(FileInfo tjaFile, int id)
        {
            Song song = null;
            _id = id;
            try
            {
                _tjaFileContents = File.ReadAllLines(tjaFile.FullName, Encoding.GetEncoding(932)).ToList();

                if (_tjaFileContents.Count > 0)
                {
                    song = TjaToSong();
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

        public TjaProcessor(int categoryId)
        {
            _categoryId = categoryId;
        }

        /// <summary>
        /// Generate a <see cref="Song"/> object based on a list of strings produced from a tja file
        /// </summary>
        private Song TjaToSong()
        {
            var title = GetStringValue("title").Replace("feat", "ft");

            Song song = new Song
            {
                Id = _id,
                CategoryId = _categoryId,
                Title = GetStringValue("title").Replace("feat", "ft"),
                Subtitle = GetStringValue("subtitle"),
                Order = _id,
                Preview = GetDoubleValue("demostart"),
                Type = SongTypeEnum.Tja.ToString().ToLower(),
                // Offset = GetDoubleValue("offset"),
                Offset = 0,
                TitleLang = GetLanguageStrings("title"),
                SubtitleLang = GetLanguageStrings("subtitle"),
                Courses = GetCourses()
                
            };

            double volume = GetDoubleValue("songvol");
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
            List<string> tjaList = list ?? _tjaFileContents;

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
            string value = GetStringValue(fieldName, list);

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
                en = GetStringValue($"{fieldName}en"),
                cn = GetStringValue($"{fieldName}cn"),
                tw = GetStringValue($"{fieldName}tw"),
                ja = GetStringValue($"{fieldName}ja"),
                ko = GetStringValue($"{fieldName}ko")
            };

            return strings;
        }

        private Courses GetCourses()
        {
            Courses courses = new Courses()
            {
                Easy = GetCourse(DifficultyEnum.Easy),
                Normal = GetCourse(DifficultyEnum.Normal),
                Hard = GetCourse(DifficultyEnum.Hard),
                Oni = GetCourse(DifficultyEnum.Oni),
                Ura = GetCourse(DifficultyEnum.Ura)
            };

            return courses;
        }

        private Course GetCourse(DifficultyEnum difficulty)
        {

            List<string> findCourse = _tjaFileContents.SkipWhile(line =>
                    !line.StartsWith($"course:{difficulty.ToString()}", StringComparison.InvariantCultureIgnoreCase) &&
                    !line.StartsWith($"course:{(int)difficulty}", StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            if (findCourse.Count == 0)
            {
                return null;
            }

            Course course = new Course
            {
                Stars = (int) GetDoubleValue("level", findCourse)
            };

            return course;
        }
    }
}
