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

        public Dictionary<String, String> Games = new Dictionary<String, String>();

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

                        if (!PlayerOne.ContainsKey(user))
                        {
                            PlayerOne.Add(user, message.DictionaryKey);
                            Dictionary<String, String> playerNumberDictionary = new Dictionary<String, String>
                            {
                                {
                                    "playerNumber", "1"
                                }
                            };

                            SendMessage(message.DictionaryKey, playerNumberDictionary.ToJson());
                            Console.WriteLine("Player One Added: " + user);
                        }
                        else if (!PlayerTwo.ContainsKey(user))
                        {
                            PlayerTwo.Add(user, message.DictionaryKey);
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

                        if (Games.ContainsKey(user) && GameHosts.ContainsKey(Games[user]))
                            SendMessage(GameHosts[Games[user]], message.JsonString);

                        break;

                    case GameHostCommand:
                        user = new JavaScriptSerializer().Deserialize<JsonUser>(message.JsonString).User;

                        if (String.IsNullOrEmpty(user)) return;

                        if (Games.ContainsKey(user) ||
                            !GameHosts.ContainsKey(message.DictionaryKey)) return;

                        Games.Add(user, message.DictionaryKey);

                        break;
                }
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

        private void SendMessage(UserContext userContext, string message)
        {
            try
            {
                userContext.Send(message);
            }
            catch (Exception)
            {
                // Remove client
                //if (GameHosts.ContainsKey(userContext.ClientAddress.ToString()))
                //    GameHosts.Remove(userContext.ClientAddress.ToString());
            }
        }
    }
}