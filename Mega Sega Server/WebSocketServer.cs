using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Mega_Sega_Server
{
    /*
        This class was used for experimenation and is not part of the final release.
    */
    public class WebSocketServer
    {
        private const string Guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        private static readonly SHA1 Sha1 = SHA1CryptoServiceProvider.Create();
        private readonly Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        public WebSocketServer()
        {
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8089));
            _serverSocket.Listen(128);
            _serverSocket.BeginAccept(null, 0, OnAccept, null);
            

            Console.WriteLine("Web Socket started.");
        }

        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        private static string AcceptKey(ref string key)
        {
            string longKey = key + Guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        private static byte[] ComputeHash(string str)
        {
            return Sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
        }

        private void OnAccept(IAsyncResult result)
        {
            Console.WriteLine("connection started.");

            byte[] buffer = new byte[10480];
            try
            {
                Socket client = null;
                string headerResponse = String.Empty;

                if (_serverSocket != null && _serverSocket.IsBound)
                {
                    client = _serverSocket.EndAccept(result);
                    var i = client.Receive(buffer);
                    headerResponse = (Encoding.UTF8.GetString(buffer)).Substring(0, i);
                    // write received data to the console
                    Console.WriteLine(headerResponse);
                }

                if (client == null) return;

                /* Handshaking and managing ClientSocket */

                var key = headerResponse.Replace("ey:", "`")
                    .Split('`')[1]                     // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                    .Replace("\r", "").Split('\n')[0]  // dGhlIHNhbXBsZSBub25jZQ==
                    .Trim();

                // key should now equal dGhlIHNhbXBsZSBub25jZQ==
                var acceptedKey = AcceptKey(ref key);

                const string newLine = "\r\n";

                var response = "HTTP/1.1 101 Switching Protocols" + newLine
                               + "Upgrade: websocket" + newLine
                               + "Connection: Upgrade" + newLine
                               + "Sec-WebSocket-Accept: " + acceptedKey + newLine + newLine
                    //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                    //+ "Sec-WebSocket-Version: 13" + newLine
                    ;

                // which one should I use? none of them fires the onopen method
                client.Send(Encoding.UTF8.GetBytes(response));
                

                var message = client.Receive(buffer); // wait for client to send a message

                string messagesString = Encoding.UTF8.GetString(buffer).Substring(0, message);
                Console.WriteLine(messagesString);

                // once the message is received decode it in different formats
                Console.WriteLine(Convert.ToBase64String(buffer).Substring(0, message));

                Console.WriteLine("\n\nPress enter to send data to client");
                Console.Read();

                client.Send(Encoding.UTF8.GetBytes("Hello yolo"));
                Console.WriteLine("Sent");
                Thread.Sleep(10000);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            finally
            {
                if (_serverSocket != null && _serverSocket.IsBound)
                {
                    _serverSocket.BeginAccept(null, 0, OnAccept, null);
                }
            }
        }
    }
}
