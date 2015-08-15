using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace DevDoodleChatbot
{
    class Program
    {
        static ManualResetEvent mre = new ManualResetEvent(false);
        static Chatbot bot;
        static void Main()
        {
            Console.Write(Properties.Resources.username);
            string name = Console.ReadLine();
            Console.Write(Properties.Resources.password);
            string pass = Console.ReadLine();
            Console.Write(Properties.Resources.roomNumber);
            int room = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
            Console.Write(Properties.Resources.prefix);
            string prefix = Console.ReadLine();
            Console.Write(Properties.Resources.owner);
            string owner = Console.ReadLine();
            Console.Clear();
            bot = new Chatbot();
            bot.ExitRequested += Bot_ExitRequested;
            bot.Start(room, name, pass, prefix, owner);
            bot.ChatRoom.OnChatEvent += ChatRoom_OnChatEvent;
            Console.WriteLine(Properties.Resources.botStarted);
            mre.WaitOne();
        }

        private static void ChatRoom_OnChatEvent(object sender, ChatEventArgs e)
        {
            // for logging
            Console.WriteLine(e.Json);
        }

        private static void Bot_ExitRequested()
        {
            bot.Dispose();
            mre.Set();
        }
    }
}
