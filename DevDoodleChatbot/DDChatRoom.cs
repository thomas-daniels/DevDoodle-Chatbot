using System;
using System.Collections.Generic;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Net;

namespace DevDoodleChatbot
{
    class DDChatRoom
    {
        public int ID
        {
            get;
            private set;
        }
        WebSocket ws = null;
        List<KeyValuePair<string, string>> cookies = new List<KeyValuePair<string, string>>();

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
            ID = id;
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
            headers.Add(new KeyValuePair<string, string>("Referer", "https://devdoodle.net/chat/" + ID));
            ws = new WebSocket("wss://devdoodle.net:81/chat/" + ID, "", cookies, headers);
            ws.MessageReceived += delegate (object sender, MessageReceivedEventArgs e)
            {
                string json = e.Message;
                Dictionary<string, string> parsedJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                ChatEventArgs cea = new ChatEventArgs(json, parsedJson);
                if (chatEvent != null)
                {
                    chatEvent(this, cea);
                }
            };
            ws.Open();
        }

        public void Send(string message)
        {
            Dictionary<string, string> msg = new Dictionary<string, string>()
            {
                { "event", "post" },
                { "body", message }
            };
            string json = JsonConvert.SerializeObject(msg);
            ws.Send(json);
        }
    }
}
