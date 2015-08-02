using System;
using System.Collections.Generic;
using System.Linq;

namespace DevDoodleChatbot
{
    class Chatbot
    {
        Dictionary<string, Func<string[], string>> Commands = new Dictionary<string, Func<string[], string>>();

        public DDClient ChatClient
        {
            get;
            private set;
        }

        public DDChatRoom ChatRoom
        {
            get;
            private set;
        }

        public string Prefix
        {
            get;
            set;
        }

        public Chatbot()
        {
            ChatClient = new DDClient();
            Commands.Add("alive", Command_Alive);
        }

        string Command_Alive(string[] args)
        {
            return "Yes, I'm alive.";
        }

        public void Start(int roomId, string username, string password, string prefix)
        {
            Prefix = prefix;
            ChatClient.Login(username, password);
            ChatRoom = ChatClient.GetRoom(roomId);
            ChatRoom.OnChatEvent += ChatRoom_OnChatEvent;
            ChatRoom.Watch();
        }

        private void ChatRoom_OnChatEvent(object sender, ChatEventArgs e)
        {
            if (e.ParsedJson["event"] != "add")
                return;
            if (!e.ParsedJson["body"].StartsWith(Prefix))
                return;
            string commandText = e.ParsedJson["body"].Remove(0, Prefix.Length);
            string[] commandWithArgs = commandText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandWithArgs.Length == 0)
                return;
            string command = commandWithArgs[0];
            string[] args = commandWithArgs.Skip(1).ToArray();
            string output = string.Empty;
            if (Commands.ContainsKey(command))
            {
                output = Commands[command](args);
            }
            else
            {
                output = "Command not found.";
            }
            output = string.Format("@{0} {1}", e.ParsedJson["user"], output);
            ChatRoom.Send(output);
        }
    }
}
