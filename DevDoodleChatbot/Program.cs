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
            Console.Clear();
            Chatbot bot = new Chatbot();
            bot.ExitRequested += Bot_ExitRequested;
            bot.Start(1, name, pass, ">>", "ProgramFOX");
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
