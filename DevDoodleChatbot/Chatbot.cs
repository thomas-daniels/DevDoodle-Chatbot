using System;
using System.Collections.Generic;
using System.Linq;

namespace DevDoodleChatbot
{
    class Chatbot
    {
        Dictionary<string, Func<string[], CommandOutput>> Commands = new Dictionary<string, Func<string[], CommandOutput>>();
        Dictionary<string, Func<string[], CommandOutput>> OwnerCommands = new Dictionary<string, Func<string[], CommandOutput>>();

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

        CommandOutput Command_Alive(string[] args)
        {
            return new CommandOutput("Yes, I'm alive.", true);
        }

        CommandOutput Command_Stop(string[] args)
        {
            if (_exitRequested != null)
            {
                _exitRequested.Invoke();
            }
            return new CommandOutput("Bot terminated.", true);
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
            CommandOutput output = null;
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
                    output = new CommandOutput("You don't have the privilege to execute this command.", true);
                }
            }
            else
            {
                output = new CommandOutput("Command not found.", true);
            }
            string s = string.Empty;
            if (output.Ping)
            {
                s = string.Format("@{0} {1}", e.ParsedJson["user"], output.Output);
            }
            else
            {
                s = output.Output;
            }
            ChatRoom.Send(s);
        }
    }
}
