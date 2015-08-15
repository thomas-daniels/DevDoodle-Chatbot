namespace DevDoodleChatbot
{
    public class CommandOutput
    {
        public string Output
        {
            get;
            private set;
        }
        public bool Ping
        {
            get;
            private set;
        }

        public CommandOutput(string output, bool ping)
        {
            Output = output;
            Ping = ping;
        }
    }
}
