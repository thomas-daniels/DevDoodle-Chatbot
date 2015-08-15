using System;
using System.Collections.Generic;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Net;

namespace DevDoodleChatbot
{
    public class DDChatRoom : IDisposable
    {
        public int Id
        {
            get;
            private set;
        }
        WebSocket ws = null;
        List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>();
        bool disposed = false;

        EventHandler<ChatEventArgs> chatEvent;
        public event EventHandler<ChatEventArgs> OnChatEvent
        {
            add
            {
                chatEvent += value;
            }
            remove
            {
                chatEvent -= value;
            }
        }

        internal DDChatRoom(int id, CookieCollection cookieCollection)
        {
            Id = id;
            foreach (Cookie c in cookieCollection)
            {
                if (c.Name == "id")
                {
                    cookies.Add(new KeyValuePair<string, string>(c.Name, c.Value));
                }
            }
            cookies[0] = new KeyValuePair<string, string>("id", WebUtility.UrlDecode(cookies[0].Value));
        }

        public void Watch()
        {
            List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
            headers.Add(new KeyValuePair<string, string>("Referer", "https://devdoodle.net/chat/" + Id));
            ws = new WebSocket("wss://devdoodle.net:81/chat/" + Id, "", cookies, headers);
            ws.MessageReceived += MessageReceived;
            ws.Open();
        }

        protected void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (disposed || e == null)
                return;
            string json = e.Message;
            Dictionary<string, string> parsedJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            ChatEventArgs cea = new ChatEventArgs(json, parsedJson);
            if (chatEvent != null)
            {
                chatEvent(this, cea);
            }
        }

        public void Send(string message)
        {
            if (disposed)
                throw new ObjectDisposedException("DDChatRoom");
            Dictionary<string, string> msg = new Dictionary<string, string>()
            {
                { "event", "post" },
                { "body", message }
            };
            string json = JsonConvert.SerializeObject(msg);
            ws.Send(json);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ws.MessageReceived -= MessageReceived;
                    ws.Dispose();
                    cookies.Clear();
                    cookies = null;
                    ws = null;
                }
                disposed = true;
            }
        }
    }
}
