
namespace Server
{
    public class Program
    {
        public static void Main()
        {
            var p = new Program();
            p.Run();
        }

        public void Run()
        {
            Console.WriteLine("<>Server shell<>");
            HttpServer server = null;
            Thread thread = null;
            ConsoleLogger logger = new ConsoleLogger();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                var command = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                switch (command)
                {
                    case "start":
                        if(thread != null && server != null && server.IsRunning)
                        {
                            Console.WriteLine("<>Server shell<>");
                            Console.WriteLine("Already running!");
                            continue;
                        }
                        server = new HttpServer(logger, ServerSettings.GetServerSettingsFromFile(logger));
                        thread = new Thread(server.Start);
                        thread.IsBackground = true;
                        thread.Start();
                        break;
                    case "restart":
                        if(thread == null || server == null)
                        {
                            Console.WriteLine("<>Server shell<>");
                            Console.WriteLine("Server is not running, use <start> command.");
                            continue;
                        }
                        server.Stop();
                        server = new HttpServer(logger, ServerSettings.GetServerSettingsFromFile(logger));
                        thread = new Thread(server.Start);
                        thread.IsBackground = true;
                        thread.Start();
                        break;
                    case "stop":
                        server.Stop();
                        thread.Join();
                        thread = null;
                        break;

                    default:
                        Console.WriteLine("<>Server shell<>");
                        Console.WriteLine("Unknown command! Use only \ncommands:\nstart\nstop\nrestart\n");
                        break;
                }
            }
        }
    }
}




