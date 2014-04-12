using System;
using System.Threading;

namespace Mega_Sega_Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // Expand thread pool
            int minWorker, minIoc;
            ThreadPool.GetMinThreads(out minWorker, out minIoc);
            ThreadPool.SetMinThreads(1000, minIoc);

            Server server = new Server();

            while (true)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "count":
                        Console.WriteLine(server.ClientHandler.Clients.Count);
                        break;
                    case "one":
                        Console.WriteLine(server.ClientHandler.PlayerOne.Count);
                        break;
                    case "two":
                        Console.WriteLine(server.ClientHandler.PlayerTwo.Count);
                        break;
                    case "hosts":
                        Console.WriteLine(server.ClientHandler.GameHosts.Count);
                        break;
                    case "games":
                        Console.WriteLine(server.ClientHandler.Games.Count);
                        break;
                    case "quit":
                        server = null;
                        break;
                }
            }
        }
    }
}