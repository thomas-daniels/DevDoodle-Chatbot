using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CsQuery;
using System.Net;
using System.Globalization;

namespace DevDoodleChatbot
{
    class Chatbot : IDisposable
    {
        Dictionary<string, Func<string[], CommandOutput>> Commands = new Dictionary<string, Func<string[], CommandOutput>>();
        Dictionary<string, Func<string[], CommandOutput>> OwnerCommands = new Dictionary<string, Func<string[], CommandOutput>>();
        bool disposed = false;

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
            Commands.Add("random", Command_Random);
            Commands.Add("randomint", Command_RandomInt);
            Commands.Add("randomchoice", Command_RandomChoice);
            Commands.Add("shuffle", Command_Shuffle);
            Commands.Add("listcommands", Command_ListCommands);
            Commands.Add("xkcd", Command_Xkcd);
            Commands.Add("help", Command_Help);
            Commands.Add("utc", Command_Utc);
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

        Random rnd = new Random();
        CommandOutput Command_RandomInt(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandOutput(rnd.Next().ToString(CultureInfo.InvariantCulture), true);
            }
            else if (args.Length == 2)
            {
                int n1;
                int n2;
                if (int.TryParse(args[0], out n1) && int.TryParse(args[1], out n2))
                {
                    if (n1 > n2)
                    {
                        return new CommandOutput("The minimum cannot be greater than the maximum.", true);
                    }
                    return new CommandOutput(rnd.Next(n1, n2).ToString(CultureInfo.InvariantCulture), true);
                }
                else
                {
                    return new CommandOutput("Invalid arguments.", true);
                }
            }
            return new CommandOutput(string.Format("0 or 2 arguments expected, {0} given.", args.Length), true);
        }

        CommandOutput Command_Random(string[] args)
        {
            return new CommandOutput(rnd.NextDouble().ToString(CultureInfo.InvariantCulture), true);
        }

        CommandOutput Command_RandomChoice(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandOutput("You didn't provide any arguments.", true);
            }
            string chosen = args[rnd.Next(args.Length)];
            return new CommandOutput(chosen, true);
        }

        CommandOutput Command_Shuffle(string[] args)
        {
            if (args.Length == 0)
            {
                return new CommandOutput("You didn't provide any arguments.", true);
            }
            for (int i = 0; i < args.Length; i++)
            {
                int j = rnd.Next(args.Length);
                string temp = args[j];
                args[j] = args[i];
                args[i] = temp;
            }
            return new CommandOutput(string.Join(" ", args), true);
        }

        CommandOutput Command_ListCommands(string[] args)
        {
            return new CommandOutput(string.Join(", ", Commands.Keys.OrderBy(x => x)), true);
        }

        CommandOutput Command_Xkcd(string[] args)
        {
            if (args.Length != 1)
            {
                return new CommandOutput("1 argument expected.", true);
            }
            if (args[0] == "404")
            {
                return new CommandOutput("404 is not a valid comic ID: http://xkcd.com/404 leads to an error page.", true);
            }
            int id;
            if (args[0] == "now" || int.TryParse(args[0], out id))
            {
                CQ middleContainer = null;
                int errorCode = -1;
                try
                {
                    CQ document = CQ.CreateFromUrl("http://xkcd.com/" + args[0]);
                    middleContainer = document.Select("#middleContainer");
                }
                catch (WebException we)
                {
                    errorCode = (int)((HttpWebResponse)we.Response).StatusCode;
                }
                if (middleContainer == null)
                {
                    return new CommandOutput(errorCode == -1 ? "Comic not found." : "Server returned error. Status code: " + errorCode, true);
                }
                string middleContainerText = middleContainer[0].InnerHTML;
                string lastLine = middleContainerText.Split(new string[] { "<br>", "</br>", "<br />" }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Last()
                                                     .Split(new string[] { "<div" }, StringSplitOptions.None)[0]
                                                     .Trim();
                string imageUrl = lastLine.Split(' ')[4];
                string escapedUrl = Regex.Replace(imageUrl, @"([_\(\)])", @"\$1");
                string markdown = string.Format("![http://xkcd.com/{0}]({1})", args[0], escapedUrl);
                return new CommandOutput(markdown, false);
            }
            return new CommandOutput("Invalid arguments.", true);
        }

        CommandOutput Command_Utc(string[] args)
        {
            return new CommandOutput(string.Format("{0} {1}", DateTime.UtcNow.ToLongDateString(), DateTime.UtcNow.ToLongTimeString()), true);
        }

        Dictionary<string, string> help = new Dictionary<string, string>()
        {
            { "alive", "Checks whether the bot is alive. It will simply post a reply. Syntax: `{0}alive`" },
            { "help", "Gets help, general or for a command. Syntax: `{0}help [ <command> ]`" },
            { "listcommands", "Gets a list of commands. Syntax: `{0}listcommands`" },
            { "randomint", "Returns a random int, optionally within an inclusive lower bound and exclusive upper bound. Syntax: `>>randomint [ <lower> <upper> ]`" },
            { "random", "Returns a random double between 0.0 and 1.0. Syntax: `>>random`" },
            { "randomchoice", "Picks a random item from a list. Syntax: `>>randomchoice 1 [ 2 [ 3 [ 4 ... ] ] ]`" },
            { "shuffle", "Shuffles a list. Syntax: `>>shuffle 1 [ 2 [ 3 [ 4 ... ] ] ]`" },
            { "xkcd", "Displays the specified xkcd comic. Syntax: `>>xkcd <comic>`" },
            { "stop", "Only for bot owners. Stops the bot. Syntax: `>>stop`" }
        };
        CommandOutput Command_Help(string[] args)
        {
            if (args.Length > 1)
            {
                return new CommandOutput("0 or 1 argument(s) expected.", true);
            }
            if (args.Length == 0)
            {
                return new CommandOutput(string.Format("I'm a chatbot. To see a list of commands, run `{0}listcommands`. To get help for a specific command, run `{0}help <command>`.",
                    Prefix), true);
            }
            else
            {
                if (help.ContainsKey(args[0]))
                {
                    return new CommandOutput(string.Format(help[args[0]], Prefix), true);
                }
                else
                {
                    return new CommandOutput("No help entry found.", true);
                }
            }
        }

        public void Start(int roomId, string username, string password, string prefix, params string[] owners)
        {
            if (disposed)
                throw new ObjectDisposedException("Chatbot");

            Prefix = prefix;
            Owners = owners;
            ChatClient.Login(username, password);
            ChatRoom = ChatClient.GetRoom(roomId);
            ChatRoom.OnChatEvent += ChatRoom_OnChatEvent;
            ChatRoom.Watch();
        }

        private void ChatRoom_OnChatEvent(object sender, ChatEventArgs e)
        {
            if (disposed)
                return;

            if (e.ParsedJson["event"] != "add")
                return;
            if (!e.ParsedJson["body"].StartsWith(Prefix))
                return;
            string commandText = e.ParsedJson["body"].Remove(0, Prefix.Length);
            string[] commandWithArgs = commandText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandWithArgs.Length == 0)
                return;
            string command = commandWithArgs[0].ToLowerInvariant();
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

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                ChatRoom.OnChatEvent -= ChatRoom_OnChatEvent;
                ChatRoom.Dispose();
                ChatRoom = null;
            }
        }
    }
}
