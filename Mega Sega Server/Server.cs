using Alchemy;
using Alchemy.Classes;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Mega_Sega_Server
{
    public class Server
    {
        public readonly ClientHandler ClientHandler = new ClientHandler();

        private const string EndTerminator = "e958248b";
        private const int MessageSize = 10480;

        private readonly TcpListener _tcpListener;

        private Alchemy.WebSocketServer _server;

        public Server()
        {
            // Start listening
            _tcpListener = new TcpListener(IPAddress.Any, 7331);
            new Thread(ListenForClients).Start();

            StartGameHostServer();

            Console.WriteLine("Server started.");
        }

        private void HandleClientComm(object client)
        {
            // Cast Client
            TcpClient tcpClient = (TcpClient)client;

            // Add Client
            string dictionaryKey = Guid.NewGuid().ToString();

            if (ClientHandler.Clients.ContainsKey(dictionaryKey))
                ClientHandler.Clients.Remove(dictionaryKey);

            ClientHandler.Clients.Add(dictionaryKey, tcpClient);

            try
            {
                NetworkStream clientStream = tcpClient.GetStream();

                byte[] message = new byte[MessageSize];

                while (true)
                {
                    int bytesRead;

                    try
                    {
                        // Blocks until message is sent to server
                        bytesRead = clientStream.Read(message, 0, MessageSize);
                    }
                    catch (Exception exception)
                    {
                        // Socket Error Occurred
                        Console.WriteLine(exception.ToString());
                        break;
                    }

                    // Client Disconnected
                    if (bytesRead == 0) break;

                    // Message Received
                    string messagesString = new ASCIIEncoding().GetString(message, 0, bytesRead);

                    // Validate Message
                    if (String.IsNullOrEmpty(messagesString) || !messagesString.Contains(EndTerminator)) continue;

                    // Handle Message(s)
                    foreach (string messageString in Regex.Split(messagesString, EndTerminator)
                        .Where(messageString => !String.IsNullOrEmpty(messageString)))
                    {
                        ThreadPool.QueueUserWorkItem(ClientHandler.HandleCommand, new Message(dictionaryKey, messageString));
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                try
                {
                    if (ClientHandler.Clients.ContainsKey(dictionaryKey))
                        ClientHandler.Clients.Remove(dictionaryKey);

                    if (ClientHandler.PlayerOne.ContainsKey(dictionaryKey))
                        ClientHandler.PlayerOne.Remove(dictionaryKey);

                    if (ClientHandler.PlayerTwo.ContainsKey(dictionaryKey))
                        ClientHandler.PlayerTwo.Remove(dictionaryKey);

                    tcpClient.Close();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }
            }
        }

        private void ListenForClients()
        {
            _tcpListener.Start();

            while (true)
            {
                // Blocks until user connects
                TcpClient client = _tcpListener.AcceptTcpClient();
                new Thread(HandleClientComm).Start(client);
            }
        }

        private void OnConnect(UserContext context)
        {
            Console.WriteLine("Connected.");

            if (!ClientHandler.GameHosts.ContainsKey(context.ClientAddress.ToString()))
                ClientHandler.GameHosts.Add(context.ClientAddress.ToString(), context);
        }

        private void OnDisconnect(UserContext context)
        {
            Console.WriteLine("Disconnect.");

            string dictionaryKey = context.ClientAddress.ToString();

            if (ClientHandler.Games.Values.Contains(dictionaryKey))
            {
                string key = ClientHandler.Games.First(pair => pair.Value.Equals(dictionaryKey)).Key;

                if (!String.IsNullOrEmpty(key) && ClientHandler.Games.ContainsKey(key))
                {
                    ClientHandler.Games.Remove(key);
                }
            }

            if (ClientHandler.GameHosts.ContainsKey(dictionaryKey))
            {
                ClientHandler.GameHosts.Remove(dictionaryKey);
            }

            _server.Stop();
            _server.Dispose();
        }

        private void OnReceive(UserContext context)
        {
            ThreadPool.QueueUserWorkItem(ClientHandler.HandleCommand, new Message(context.ClientAddress.ToString(), context.DataFrame.ToString()));
        }

        private void StartGameHostServer()
        {
            _server = new Alchemy.WebSocketServer(8089, IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnConnected = OnConnect,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(1, 0, 0, 0)
            };

            _server.Start();

            Console.WriteLine("Web Sockets server started.");
        }

        private void RestartSocketServer()
        {
            _server.Stop();
            _server.Start();
        }
    }
}