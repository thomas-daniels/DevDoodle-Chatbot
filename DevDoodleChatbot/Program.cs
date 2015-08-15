using System;
using System.Threading;

namespace DevDoodleChatbot
{
    class Program
    {
        static ManualResetEvent mre = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            Console.Write("Username: ");
            string name = Console.ReadLine();
            Console.Write("Password: ");
            string pass = Console.ReadLine();
            Console.Write("Room number: ");
            int room = int.Parse(Console.ReadLine());
            Console.Write("Prefix: ");
            string prefix = Console.ReadLine();
            Console.Write("Owner name: ");
            string owner = Console.ReadLine();
            Console.Clear();
            Chatbot bot = new Chatbot();
            bot.ExitRequested += Bot_ExitRequested;
            bot.Start(room, name, pass, prefix, owner);
            bot.ChatRoom.OnChatEvent += ChatRoom_OnChatEvent;
            Console.WriteLine("Bot started.");
            mre.WaitOne();
        }

        private static void ChatRoom_OnChatEvent(object sender, ChatEventArgs e)
        {
            // for logging
            Console.WriteLine(e.Json);
        }

        private static void Bot_ExitRequested()
        {
            mre.Set();
        }
    }
}
