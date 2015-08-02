using System;

namespace DevDoodleChatbot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Username: ");
            string name = Console.ReadLine();
            Console.Write("Password: ");
            string pass = Console.ReadLine();
            Console.Clear();
            Chatbot bot = new Chatbot();
            bot.Start(1, name, pass, ">>");
            Console.WriteLine("Bot started.");
            while (true)
            {
                Console.Write("<< ");
                bot.ChatRoom.Send(Console.ReadLine());
            }
        }
    }
}
