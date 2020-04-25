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
            this._directory = directory;
            this._startId = startId;
            this._categoryId = categoryId;
            this._outputDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).CreateSubdirectory($"Output/Category {categoryId}/");
            this._generateMarkers = generateMarkerFiles;
        }

        public void ProcessDirectory()
        {
            var subDirectories = this._directory.GetDirectories().Where(dir => dir.ContainsSong()).ToList();
            var oszSongs = this._directory.GetOszFiles();

            if (this._outputDirectory.GetDirectories().Length > 0)
            {
                Console.WriteLine("Cleaning up output directory..");
                foreach (DirectoryInfo directoryInfo in this._outputDirectory.GetDirectories())
                {
                    directoryInfo.Delete(true);
                }
            }

            int total = subDirectories.Count + oszSongs.Length;
            int id = this._startId;
            int count = 1;
            int succesful = 0;

            string format = $"D{total.ToString().Length}";
            Console.WriteLine($"Processing {total} songs!");

            if (oszSongs.Any())
            {
                DirectoryInfo tempDirectory = this._outputDirectory.CreateSubdirectory("temp");

                OsuProcessor osuProcessor = new OsuProcessor(this._categoryId, tempDirectory);
                foreach (FileInfo fileInfo in oszSongs)
                {
                    Console.Write(
                        $"[{count.ToString(format)}/{total.ToString()}] {Path.GetFileNameWithoutExtension(fileInfo.FullName)}..");

                    Song newSong = osuProcessor.Process(fileInfo, id);
                    if (newSong != null)
                    {
                        this._songs.Add(newSong);

                        string outputPath = $"{this._outputDirectory.FullName}{Path.DirectorySeparatorChar}{id}";

                        Directory.CreateDirectory(outputPath);

                        foreach (FileInfo enumerateFile in tempDirectory.EnumerateFiles())
                        {
                            enumerateFile.MoveTo($"{outputPath}{Path.DirectorySeparatorChar}{enumerateFile.Name}");
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
                TjaProcessor tjaProcessor = new TjaProcessor(this._categoryId);
                foreach (DirectoryInfo subDirectory in subDirectories)
                {
                    FileInfo tjaFile = subDirectory.GetTjaFile();
                    FileInfo musicFile = subDirectory.GetMusicFile();

                    Console.Write(
                        $"[{count.ToString(format)}/{total.ToString()}] {Path.GetFileNameWithoutExtension(tjaFile.FullName)}..");

                    Song newSong = tjaProcessor.Process(tjaFile, id);

                    if (newSong != null)
                    {
                        this._songs.Add(newSong);
                    
                    
                        string outputPath = $"{this._outputDirectory.FullName}{Path.DirectorySeparatorChar}{id}";

                        Directory.CreateDirectory(outputPath);

#if !DEBUG
                musicFile.CopyTo($"{outputPath}{Path.DirectorySeparatorChar}main.ogg",true);
#endif

                        tjaFile.CopyTo($"{outputPath}{Path.DirectorySeparatorChar}main.tja", true);

                        if (this._generateMarkers) //behind a switch for now since I don't want to piss off my FTP server (yet)
                        {
                            HepburnConverter hepburn = new HepburnConverter();
                            string markerFile =
                                $"{outputPath}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(WanaKana.ToRomaji(hepburn, tjaFile.FullName))}";

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

            string json = JsonSerializer.Serialize(this._songs);
            Console.WriteLine("Exporting json...");
            File.WriteAllText($@"{this._outputDirectory}{Path.DirectorySeparatorChar}songs.json", json, Encoding.GetEncoding(932));

            Console.WriteLine($"\nDone! Enjoy! Don't forget to import songs.json to mongoDB!");
        }
    }
}
