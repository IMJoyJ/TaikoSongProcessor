using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using IniParser.Parser;
using TaikoSongProcessor.Lib.Extensions;
using TaikoSongProcessor.Lib.Models;

namespace TaikoSongProcessor.Lib
{
    public class OsuProcessor
    {
        private readonly int _categoryId;
        private int _id;
        private IniDataParser _dataParser;
        private DirectoryInfo _tempDirectory;
        private const int TAIKOMODE = 1;

        public OsuProcessor(int categoryId, DirectoryInfo tempDirectory)
        {
            _categoryId = categoryId;
            _dataParser = new IniDataParser(new IniParserConfiguration
            {
                SkipInvalidLines = true,
                KeyValueAssigmentChar = ':',
                CaseInsensitive = true
            });

            _tempDirectory = tempDirectory;
        }

        public Song Process(FileInfo archive, int id)
        {
            _id = id;

            //empty temp folder
            foreach (FileInfo enumerateFile in _tempDirectory.EnumerateFiles())
            {
                enumerateFile.Delete();
            }
            
            Song song;

            using (ZipArchive zip = ZipFile.OpenRead(archive.FullName))
            {
                //check if the zip contains an mp3 file
                ZipArchiveEntry mp3File = zip.Entries.FirstOrDefault(entry =>
                    entry.Name.EndsWith(".mp3", StringComparison.InvariantCultureIgnoreCase));

                if (mp3File == null)
                {
                    ConsoleHelper.WriteError("Archive contains no mp3 file!");
                    return null;
                }

                //grab the first beatmap
                List<ZipArchiveEntry> containsBeatmaps = zip.Entries.Where(entry =>
                    entry.Name.Contains(".osu", StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (!containsBeatmaps.Any())
                {
                    ConsoleHelper.WriteError("Archive contains no beatmaps!");
                    return null;
                }

                List<ZipArchiveEntry> beatmaps = containsBeatmaps.Where(ContainsTaikoBeatmap).ToList();

                if (!beatmaps.Any())
                {
                    ConsoleHelper.WriteError("Archive contains no Osu!Taiko beatmaps!!");
                    return null;
                }

                song = GetSongData(GetIniData(beatmaps.FirstOrDefault())); //initial load for the metadata
                if (song != null)
                {
                    song.Courses = ProcessCourses(beatmaps);
                    if (song.Courses != null)
                    {
                        mp3File.ExtractToFile($"{_tempDirectory}\\main.mp3");
                    }
                    else
                    {
                        ConsoleHelper.WriteError("Failed to find courses!");
                        song = null;
                    }
                }
            }

            return song;
        }

        private bool ContainsTaikoBeatmap(ZipArchiveEntry file)
        {
            IniData data = GetIniData(file);

            return string.Equals(
                data.GetKey("general.mode"), 
                TAIKOMODE.ToString(),
                StringComparison.InvariantCultureIgnoreCase);
        }

        private Courses ProcessCourses(List<ZipArchiveEntry> beatmaps)
        {
            Courses courses = new Courses();

            Regex easyRegex = new Regex(DifficultyLabels.EasyRegex, RegexOptions.IgnoreCase);
            Regex normalRegex = new Regex(DifficultyLabels.NormalRegex, RegexOptions.IgnoreCase);
            Regex hardRegex = new Regex(DifficultyLabels.HardRegex, RegexOptions.IgnoreCase);
            Regex oniRegex = new Regex(DifficultyLabels.OniRegex, RegexOptions.IgnoreCase);
            Regex uraRegex = new Regex(DifficultyLabels.UraRegex, RegexOptions.IgnoreCase);

            foreach (ZipArchiveEntry archiveEntry in beatmaps)
            {
                string filename = archiveEntry.Name;
                
                if (easyRegex.Match(filename).Success)
                {
                    if (courses.Easy != null) continue;
                    courses.Easy = ProcessCourse(archiveEntry, DifficultyEnum.Easy);
                }else if (normalRegex.Match(filename).Success)
                {
                    if (courses.Normal != null) continue;
                    courses.Normal = ProcessCourse(archiveEntry, DifficultyEnum.Normal);
                }
                else if (hardRegex.Match(filename).Success)
                {
                    if (courses.Hard != null) continue;
                    courses.Hard = ProcessCourse(archiveEntry, DifficultyEnum.Hard);
                }
                else if (oniRegex.Match(filename).Success)
                {
                    if (courses.Oni != null) continue;
                    courses.Oni = ProcessCourse(archiveEntry, DifficultyEnum.Oni);
                }
                else if (uraRegex.Match(filename).Success)
                {
                    if (courses.Ura != null) continue;
                    courses.Ura = ProcessCourse(archiveEntry, DifficultyEnum.Ura);
                }
            }

            if (!courses.HasCourses())
            {
                courses = null;
            }

            return courses;
        }

        private Course ProcessCourse(ZipArchiveEntry zipArchiveEntry, DifficultyEnum difficulty)
        {
            Course course = new Course
            {
                Branch = false
            };
            IniData data = GetIniData(zipArchiveEntry);
            if (data.TryGetKey("difficulty.overalldifficulty", out string stars))
            {
                if (int.TryParse(stars, out int starsInt))
                {
                    course.Stars = starsInt;

                    zipArchiveEntry.ExtractToFile($"{_tempDirectory.FullName}\\{difficulty.ToString().ToLower()}.osu");

                    return course;
                }
            }
            
            return null;
        }

        private IniData GetIniData(ZipArchiveEntry zipArchiveEntry)
        {
            using StreamReader reader = new StreamReader(zipArchiveEntry.Open());
            FileIniDataParser iniParser = new FileIniDataParser(_dataParser);
            IniData iniData = iniParser.ReadData(reader);
            return iniData;
        }

        private Song GetSongData(IniData data)
        {
            Song song = new Song
            {
                Id = _id,
                CategoryId = _categoryId,
                Order = _id,
                Offset = 0,
                Volume = 1,
                Type = SongTypeEnum.Osu.ToString().ToLower()
            };

            if (!data.Sections.ContainsSection("general"))
            {
                ConsoleHelper.WriteError("Could not find general data!");
                return null;
            }

            if (data.TryGetKey("general.mode", out string modeString))
            {
                if (int.TryParse(modeString, out int mode))
                {
                    if (mode != TAIKOMODE)
                    {
                        ConsoleHelper.WriteError("Not an Osu!Taiko beatmap!!");
                        return null;
                    }
                }
            }
            else
            {
                ConsoleHelper.WriteError("Could not determine mode!");
                return null;
            }

            if (data.TryGetKey("general.previewtime", out string previewTime))
            {
                if(double.TryParse(previewTime, out double previewTimeAsDbl))
                {
                    song.Preview = previewTimeAsDbl / 1000;
                }
            }

            if (!data.Sections.ContainsSection("metadata"))
            {
                ConsoleHelper.WriteError("Could not find metadata!");
                return null;
            }

            if (data.TryGetKey("metadata.title", out string title))
            {
                song.Title = title;
            }
            else
            {
                ConsoleHelper.WriteError("Could not find title!");
                return null;
            }

            if (!string.IsNullOrWhiteSpace(data.GetKey("metadata.artist")) &&
                !string.IsNullOrWhiteSpace(data.GetKey("metadata.source")))
            {
                song.Subtitle = $"{data.GetKey("metadata.artist")} - {data.GetKey("metadata.source")}";
            }
            else if (data.GetKey("metadata.artist") != null)
            {
                song.Subtitle = data.GetKey("metadata.artist");
            }

            return song;
        }
    }
}
