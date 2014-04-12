using Alchemy.Classes;
using System;
using System.Net;

namespace Mega_Sega_Server
{
    public class GameHostServer
    {
        public GameHostServer()
        {
            Alchemy.WebSocketServer server = new Alchemy.WebSocketServer(8089, IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnConnected = OnConnect,
                OnDisconnect = OnDisconnect
            };

            server.Start();

            Console.WriteLine("Web Sockets server started.");
        }

        public void OnConnect(UserContext context)
        {
        }

        public void OnDisconnect(UserContext context)
        {
        }

        public void OnReceive(UserContext context)
        {
        }

        private void SendMessage(UserContext context, string message)
        {
            try
            {
                context.Send(message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}