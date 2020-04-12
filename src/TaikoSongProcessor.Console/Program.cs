
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TaikoSongProcessor.Lib;
using TaikoSongProcessor.Lib.Extensions;

namespace TaikoSongProcessor.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding(932); //needed to enable moonrunes

            WriteWelcomeMessage();
            WriteSongDescription();

            int startId = RequestInt("Enter song ID to start with");

            WriteCategoryDescription();

            int categoryId = RequestInt("Enter category ID");

            var generateMarkers = false;

            WriteMarkerFileDescription();

            ConsoleKey markerYesNo;

            Console.WriteLine("Do you want to generate \"marker\" files? (y/N) ");

            do
            {
                markerYesNo = Console.ReadKey(true).Key;

                if (markerYesNo.Equals(ConsoleKey.Enter) || markerYesNo.Equals(ConsoleKey.N))
                {
                    generateMarkers = false;
                    Console.Write("N");
                    Console.WriteLine();
                }
                else if(markerYesNo.Equals(ConsoleKey.Y))
                {
                    generateMarkers = true;
                    Console.Write("Y");
                    Console.WriteLine();
                }
            } while (markerYesNo != ConsoleKey.Y &&
                     markerYesNo != ConsoleKey.Enter &&
                     markerYesNo != ConsoleKey.N);

            DirectoryInfo directory = RequestDirectory("Enter folder to process");

            SongProcessor songProcessor = new SongProcessor(directory, startId, categoryId, generateMarkers);

            songProcessor.ProcessDirectory();

            await Task.Delay(2000);
        }

        #region Description texts

        private static void WriteWelcomeMessage()
        {
            Console.WriteLine("Welcome to Taiko Song Processor!");
        }

        private static void WriteSongDescription()
        {
            Console.WriteLine("Taiko-web requires a unique ID for every song in its database. Since we can't look into Taiko-web's database directly,\nplease enter an ID to start our batch with.");
            Console.WriteLine("Output folders will follow this number sequentially, so make sure you enter something that makes sense.\n");
        }

        private static void WriteCategoryDescription()
        {
            Console.WriteLine("The processor currently imports all songs to a single category. Taiko-Web has the following default categories:");

            //Originally we also showed japanese here but we can't render that properly (yet)
            Console.WriteLine("1. J-POP");
            Console.WriteLine("2. Anime");
            Console.WriteLine("3. Vocaloid™");
            Console.WriteLine("4. Variety");
            Console.WriteLine("5. Classical");
            Console.WriteLine("6. Game music");
            Console.WriteLine("7. Namco Original");

            Console.WriteLine();
        }

        private static void WriteMarkerFileDescription()
        {
            Console.WriteLine("Marker files are 0-byte files placed in each song folder to make organizing a bit easier.");
            Console.WriteLine("WARNING: Even though the program tries to convert everything to romaji, some characters still\nslip through which can make stuff like FTP a nuisance due to encoding issues.");
        }

        #endregion

        private static int RequestInt(string inputMsg, string optionalMsg = "")
        {
            int? inputInt = null;
            while (!inputInt.HasValue)
            {
                Console.Write($"{inputMsg}: ");

                var inputVal = Console.ReadLine();
                if (int.TryParse(inputVal, out int parsedInt) && parsedInt >= 0)
                {
                    inputInt = parsedInt;
                }
                else
                {
                    Console.WriteLine("Invalid input.");
                }
            }

            return inputInt.Value;
        }

        private static DirectoryInfo RequestDirectory(string inputMsg) //wtf
        {
            DirectoryInfo inputDirectory = null;
            while (inputDirectory == null)
            {
                Console.Write($"{inputMsg}: ");

                string inputVal = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(inputVal))
                {
                    Console.WriteLine("Invalid input.");
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(inputVal);
                    if (!dirInfo.Exists)
                    {
                        Console.WriteLine("Directory does not exist.");
                    }
                    else
                    {
                        Console.WriteLine($"Looking in {dirInfo.FullName}...");

                        var oszFiles = dirInfo.GetOszFiles();

                        DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                        if (subDirs.Length == 0 && oszFiles.Length == 0)
                        {
                            Console.WriteLine($"Didn't find any archives or subdirs here :(");
                        }
                        else
                        {
                            int count = 0;

                            foreach (FileInfo fileInfo in oszFiles)
                            {
                                string songName = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                                Console.WriteLine($"Found song {songName}");
                                count += 1;
                            }

                            foreach (DirectoryInfo dir in subDirs)
                            {
                                if (dir.ContainsSong())
                                {
                                    string songName = Path.GetFileNameWithoutExtension(dir.GetTjaFile().FullName);
                                    Console.WriteLine($"Found song {songName}");
                                    count += 1;
                                }
                            }

                            if (count == 0)
                            {
                                Console.WriteLine("Found zero songs to import.");
                            }
                            else
                            {
                                ConsoleKey response;
                                do
                                {
                                    Console.WriteLine($"\nFound {count} songs to import. Proceed? (Y/n): ");
                                    response = Console.ReadKey(true).Key;

                                    if (response.Equals(ConsoleKey.Enter) || response.Equals(ConsoleKey.Y))
                                    {
                                        inputDirectory = new DirectoryInfo(inputVal);
                                        Console.WriteLine();
                                    }
                                } while (response != ConsoleKey.Y && 
                                         response != ConsoleKey.Enter &&
                                         response != ConsoleKey.N);
                            }
                        }
                    }
                }
            }

            return inputDirectory;
        }
    }
}
