using System;

namespace Server
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string info)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(info);
            Console.ForegroundColor = current;
        }

        public void ReportError(string error)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR OCCURRED!");
            Log(error);
            Console.ForegroundColor = current;
            
        }
    }
}
