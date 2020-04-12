using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaikoSongProcessor.Lib.Extensions
{
    public class ConsoleHelper
    {
        public static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{msg}\n");
            Console.ResetColor();
        }
        public static void WriteLineError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{msg}");
            Console.ResetColor();
        }
    }
}
