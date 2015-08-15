using System;
using System.Runtime.Serialization;

namespace DevDoodleChatbot
{
    [Serializable]
    public class LogOnException : Exception
    {
        public LogOnException()
        {
        }

        public LogOnException(string message) : base(message)
        {
        }

        public LogOnException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LogOnException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}