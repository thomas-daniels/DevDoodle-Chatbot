using System;
using System.Collections.Generic;

namespace DevDoodleChatbot
{
    public class ChatEventArgs : EventArgs
    {
        public string Json
        {
            get;
            private set;
        }
        public Dictionary<string, string> ParsedJson
        {
            get;
            private set;
        }

        public ChatEventArgs(string json, Dictionary<string, string> parsedJson)
        {
            Json = json;
            ParsedJson = parsedJson;
        }
    }
}
