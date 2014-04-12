using System.Net.Configuration;
using Alchemy.Classes;
using Mega_Sega_Server.JsonObjects;
using Mega_Sega_Server.Utility;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;

namespace Mega_Sega_Server
{
    public class ClientHandler
    {
        public Dictionary<String, TcpClient> Clients = new Dictionary<String, TcpClient>();

        public Dictionary<String, UserContext> GameHosts = new Dictionary<String, UserContext>();

        public Dictionary<String, String> PlayerOne = new Dictionary<String, String>();
        public Dictionary<String, String> PlayerTwo = new Dictionary<String, String>();

        private const string ConnectCommand = "connect";
        private const string GameHostCommand = "host";
        private const string KeyCommand = "key";

        public void HandleCommand(object threadInfo)
        {
            Message message = threadInfo as Message;

            if (message == null || String.IsNullOrEmpty(message.JsonString)
                || String.IsNullOrEmpty(message.DictionaryKey)) return;

            try
            {
                string commandType = new JavaScriptSerializer().Deserialize<CommandType>(message.JsonString).Command;
                string user = String.Empty;

                switch (commandType)
                {
                    case ConnectCommand:
                        user = new JavaScriptSerializer().Deserialize<JsonUser>(message.JsonString).User;

                        if (String.IsNullOrEmpty(user)) return;

                        if (!PlayerOne.ContainsKey(message.DictionaryKey))
                        {
                            PlayerOne.Add(message.DictionaryKey, user);
                            Dictionary<String, String> playerNumberDictionary = new Dictionary<String, String>
                            {
                                {
                                    "playerNumber", "1"
                                }
                            };
                            SendMessage(message.DictionaryKey, playerNumberDictionary.ToJson());
                            Console.WriteLine("Player One Added: " + user);
                        }
                        else if (!PlayerTwo.ContainsKey(message.DictionaryKey))
                        {
                            PlayerTwo.Add(message.DictionaryKey, user);
                            Dictionary<String, String> playerNumberDictionary = new Dictionary<String, String>
                            {
                                {
                                    "playerNumber", "2"
                                }
                            };
                            SendMessage(message.DictionaryKey, playerNumberDictionary.ToJson());
                            Console.WriteLine("Player Two Added: " + user);
                        }

                        break;

                    case KeyCommand:
                        user = new JavaScriptSerializer().Deserialize<JsonUser>(message.JsonString).User;

                        if (String.IsNullOrEmpty(user)) return;

                        //if (GameHosts.ContainsKey(user))
                            //SendMessage(GameHosts[user], message.JsonString);

                        break;
                    case GameHostCommand:
                        user = new JavaScriptSerializer().Deserialize<JsonUser>(message.JsonString).User;

                        if (String.IsNullOrEmpty(user)) return;

                        if (GameHosts.ContainsKey(user)) return;

                        //GameHosts.Add(user, message.DictionaryKey);

                        break;
                }

                SendMessageToAll("yolo");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return;
            }
        }

        private void SendMessage(string dictionaryKey, string message)
        {
            try
            {
                if (!Clients.ContainsKey(dictionaryKey)) return;

                TcpClient client = Clients[dictionaryKey];

                NetworkStream clientStream = client.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes(message);

                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();
            }
            catch (Exception ex)
            {
                if (Clients.ContainsKey(dictionaryKey))
                    Clients.Remove(dictionaryKey);

                Console.WriteLine(ex.ToString());
            }
        }

        private void SendMessageToAll(string message)
        {
            foreach (TcpClient client in Clients.Values)
            {
                try
                {
                    NetworkStream clientStream = client.GetStream();
                    ASCIIEncoding encoder = new ASCIIEncoding();
                    byte[] buffer = encoder.GetBytes(message);

                    clientStream.Write(buffer, 0, buffer.Length);
                    clientStream.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}