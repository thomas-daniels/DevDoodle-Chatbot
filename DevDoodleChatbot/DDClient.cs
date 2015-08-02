using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebSocket4Net;
using Newtonsoft.Json;

namespace DevDoodleChatbot
{
    class DDClient
    {
        CookieCollection cookies = new CookieCollection();

        public void Login(string name, string password)
        {
            HttpWebRequest req = HttpWebRequest.CreateHttp("https://devdoodle.net/login/");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.Referer = "https://devdoodle.net/";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(cookies);

            NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
            queryString.Add("name", name);
            queryString.Add("pass", password);
            string postData = queryString.ToString();
            byte[] postBytes = Encoding.ASCII.GetBytes(postData);

            req.ContentLength = postBytes.Length;

            using (Stream stream = req.GetRequestStream())
            {
                stream.Write(postBytes, 0, postBytes.Length);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            bool success = false;
            foreach (Cookie c in resp.Cookies)
            {
                if (c.Name == "id")
                {
                    success = true;
                    break;
                }
            }
            if (!success)
            {
                throw new Exception("Failed to log in: could not get `id` cookie. Check your username and password.");
            }
            cookies = resp.Cookies;
        }

        public DDChatRoom GetRoom(int id)
        {
            return new DDChatRoom(id, cookies);
        }
    }
}
