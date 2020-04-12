using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using TaikoSongProcessor.lib;
using TaikoSongProcessor.Lib.Extensions;
using TaikoSongProcessor.Lib.Models;
using WanaKanaSharp;

namespace TaikoSongProcessor.Lib
{
    public class SongProcessor
    {
        private readonly DirectoryInfo _directory;
        private readonly int _startId;
        private readonly int _categoryId;
        private readonly bool _generateMarkers;
        private readonly DirectoryInfo _outputDirectory;
        private readonly List<Song> _songs = new List<Song>();

        public SongProcessor(DirectoryInfo directory, int startId, int categoryId, bool generateMarkerFiles = false)
        {
            _directory = directory;
            _startId = startId;
            _categoryId = categoryId;
            _outputDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory($"Output/Category {categoryId}/");
            _generateMarkers = generateMarkerFiles;
        }

        public void ProcessDirectory()
        {
            var subDirectories = _directory.GetDirectories().Where(dir => dir.ContainsSong()).ToList();
            var oszSongs = _directory.GetOszFiles();

            if (_outputDirectory.GetDirectories().Length > 0)
            {
                Console.WriteLine("Cleaning up output directory..");
                foreach (DirectoryInfo directoryInfo in _outputDirectory.GetDirectories())
                {
                    directoryInfo.Delete(true);
                }
            }

            int total = subDirectories.Count + oszSongs.Length;
            int id = _startId;
            int count = 1;
            int succesful = 0;

            string format = $"D{total.ToString().Length}";
            Console.WriteLine($"Processing {total} songs!");

            if (oszSongs.Any())
            {
                DirectoryInfo tempDirectory = _outputDirectory.CreateSubdirectory("temp");

                OsuProcessor osuProcessor = new OsuProcessor(_categoryId, tempDirectory);
                foreach (FileInfo fileInfo in oszSongs)
                {
                    Console.Write(
                        $"[{count.ToString(format)}/{total.ToString()}] {Path.GetFileNameWithoutExtension(fileInfo.FullName)}..");

                    Song newSong = osuProcessor.Process(fileInfo, id);
                    if (newSong != null)
                    {
                        _songs.Add(newSong);

                        string outputPath = $"{_outputDirectory.FullName}\\{id}";

                        Directory.CreateDirectory(outputPath);

                        foreach (FileInfo enumerateFile in tempDirectory.EnumerateFiles())
                        {
                            enumerateFile.MoveTo($"{outputPath}\\{enumerateFile.Name}");
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("OK! \n");
                        Console.ResetColor();

                        succesful += 1;
                        id += 1;
                    }

                    count += 1;
                }

                tempDirectory.Delete(true);
            }



            if (subDirectories.Any())
            {
                TjaProcessor tjaProcessor = new TjaProcessor(_categoryId);
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    FileInfo tjaFile = subDirectory.GetTjaFile();
                    FileInfo mp3File = subDirectory.GetMp3File();

                    Console.Write(
                        $"[{count.ToString(format)}/{total.ToString()}] {Path.GetFileNameWithoutExtension(tjaFile.FullName)}..");

                    Song newSong = tjaProcessor.Process(tjaFile, id);

                    if (newSong != null)
                    {
                        _songs.Add(newSong);
                    
                    
                        string outputPath = $"{_outputDirectory.FullName}\\{id}";

                        Directory.CreateDirectory(outputPath);

#if !DEBUG
                mp3File.CopyTo($"{outputPath}\\main.mp3",true);
#endif

                        tjaFile.CopyTo($"{outputPath}\\main.tja", true);

                        if (_generateMarkers) //behind a switch for now since I don't want to piss off my FTP server (yet)
                        {
                            HepburnConverter hepburn = new HepburnConverter();
                            string markerFile =
                                $"{outputPath}\\{Path.GetFileNameWithoutExtension(WanaKana.ToRomaji(hepburn, tjaFile.FullName))}";

                            if (!File.Exists(markerFile))
                            {
                                File.Create(
                                    markerFile); //create an empty file with the song name, just to keep shit organised
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("OK! \n");
                        Console.ResetColor();

                        succesful += 1;
                        id += 1;
                    }

                    count += 1;
                }
            }

            Console.WriteLine($"\nSuccesfully processed {succesful} songs out of {total}!");

            string json = JsonSerializer.Serialize(_songs);
            Console.WriteLine("Exporting json...");
            File.WriteAllText($@"{_outputDirectory}\\songs.json", json, Encoding.GetEncoding(932));

            Console.WriteLine($"\nDone! Enjoy! Don't forget to import songs.json to mongoDB!");
        }
    }
}
