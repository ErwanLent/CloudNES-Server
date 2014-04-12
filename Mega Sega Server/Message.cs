namespace Mega_Sega_Server
{
    public class Message
    {
        public Message(string key, string message)
        {
            DictionaryKey = key;
            JsonString = message;
        }

        public string DictionaryKey { get; set; }

        public string JsonString { get; set; }
    }
}