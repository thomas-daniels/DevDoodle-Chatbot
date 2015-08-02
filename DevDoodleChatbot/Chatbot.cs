using System;
using System.Collections.Generic;
using System.Linq;

namespace DevDoodleChatbot
{
    class Chatbot
    {
        Dictionary<string, Func<string[], string>> Commands = new Dictionary<string, Func<string[], string>>();
        Dictionary<string, Func<string[], string>> OwnerCommands = new Dictionary<string, Func<string[], string>>();

        Action _exitRequested;
        public event Action ExitRequested
        {
            add
            {
                _exitRequested += value;
            }
            remove
            {
                _exitRequested -= value;
            }
        }

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

        public string[] Owners
        {
            get;
            private set;
        }

        public Chatbot()
        {
            ChatClient = new DDClient();
            Commands.Add("alive", Command_Alive);
            OwnerCommands.Add("stop", Command_Stop);
        }

        string Command_Alive(string[] args)
        {
            return "Yes, I'm alive.";
        }

        string Command_Stop(string[] args)
        {
            if (_exitRequested != null)
            {
                _exitRequested.Invoke();
            }
            return "Bot terminated.";
        }

        public void Start(int roomId, string username, string password, string prefix, params string[] owners)
        {
            Prefix = prefix;
            Owners = owners;
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
            else if (OwnerCommands.ContainsKey(command))
            {
                if (Owners.Contains(e.ParsedJson["user"]))
                {
                    output = OwnerCommands[command](args);
                }
                else
                {
                    output = "You don't have the privilege to execute this command.";
                }
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
