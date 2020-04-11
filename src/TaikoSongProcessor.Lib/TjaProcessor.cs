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
            Song song;
            _id = id;
            try
            {
                _tjaFileContents = File.ReadAllLines(tjaFile.FullName, Encoding.GetEncoding(932)).ToList();
                song = TjaToSong();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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

            Console.Write($"Processing {title}..");

            Song song = new Song
            {
                Id = _id,
                CategoryId = _categoryId,
                Title = GetStringValue("title").Replace("feat", "ft"),
                Subtitle = GetStringValue("subtitle"),
                Order = _id,
                Preview = GetDoubleValue("demostart"),
                // Offset = GetDoubleValue("offset"),
                Offset = 0,
                TitleLang = GetLanguageStrings("title"),
                SubtitleLang = GetLanguageStrings("subtitle"),
                Volume = 100 / GetDoubleValue("songvol"),
                Courses = GetCourses(),
                Hash = Guid.NewGuid().ToString() //Taiko-web doesn't actually require a real hash here - any randomized poo will work. Used to id highscores.
            };

            return song;
        }

        private string GetStringValue(string fieldName, List<string> list = null)
        {
            List<string> tjaList = list ?? _tjaFileContents;

            string value = tjaList.FirstOrDefault(line => line.StartsWith(fieldName, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value?.Substring(value.IndexOf(':') + 1);
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
                    !line.StartsWith($"course:{difficulty.ToString()}", StringComparison.InvariantCultureIgnoreCase))
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
