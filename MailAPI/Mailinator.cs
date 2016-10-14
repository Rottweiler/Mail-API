using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

/*
 * 
 * Usage: 
 * 
 * 
 * Mailinator api = new Mailinator("username");
 * 
 * var inbox = api.getInboxStatus();
 * foreach(var email in inbox.public_msgs)
 * {
 *   var easy_mail = api.getEasyMail(email);
 *   Console.WriteLine("New email from: " + easy_mail.From);
 * }
 * 
 * 
 */

namespace MailAPI
{
    /// <summary>
    /// Mailinator.com API
    /// </summary>
    public class Mailinator
    {
        /// <summary>
        /// Public API status link
        /// </summary>
        private const string PUBLIC_API_STATUS = "https://www.mailinator.com/api/webinbox2?x=0&public_to={0}";

        /// <summary>
        /// Public API message link
        /// </summary>
        private const string PUBLIC_API_MESSAGE = "https://www.mailinator.com/fetchmail?msgid={0}&zone=public";

        private CookieContainer cookies;
        private string mailbox;
        private DateTime lastRequest = DateTime.Now.AddSeconds(-1);

        /// <summary>
        /// Cookies for the current session
        /// </summary>
        public CookieContainer Cookies
        {
            get { return cookies; }
            private set { cookies = value; }
        }

        /// <summary>
        /// Current username/mailbox for the session
        /// </summary>
        public string Mailbox
        {
            get { return mailbox; }
            private set { mailbox = value; }
        }

        /// <summary>
        /// Constructor .ctor (spawn new session)
        /// </summary>
        /// <param name="mailbox">Mailbox name without the @mailinator.com part</param>
        public Mailinator(string mailbox)
        {
            this.mailbox = mailbox;
            this.cookies = new CookieContainer();
            if (this.mailbox.Contains("@")) this.mailbox = this.mailbox.Split(Convert.ToChar("@"))[0];
        }

        /// <summary>
        /// Gets the inbox status/data
        /// </summary>
        /// <returns>Inbox status/data</returns>
        public MailinatorStatus getInboxStatus()
        {
            var inbox_json_1 = createRequest(
                string.Format(PUBLIC_API_STATUS, this.mailbox),
                "application/json, text/javascript, */*; q=0.01",
                "https://www.mailinator.com/inbox2.jsp",
                false,
                new KeyValuePair<string, string>("Accept-Language", "en-US,en;q=0.5"),
                new KeyValuePair<string, string>("X-Requested-With", "XMLHttpRequest")
            );

            string response = getResponse<string>(inbox_json_1);
            var inbox_data = JsonConvert.DeserializeObject<MailinatorStatus>(response);

            return inbox_data;
        }

        /// <summary>
        /// Get message data
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Message data</returns>
        public MailinatorMessage getMessageData(PublicMsg message)
        {
            return getMessageData(message.id);
        }

        /// <summary>
        /// Get message data
        /// </summary>
        /// <param name="id">Message id</param>
        /// <returns>Message data</returns>
        public MailinatorMessage getMessageData(string id)
        {
            var message_json = createRequest(
                string.Format(PUBLIC_API_MESSAGE, id),
                "*/*",
                "https://www.mailinator.com/inbox2.jsp?public_to=" + this.mailbox,
                true,
                new KeyValuePair<string, string>("Accept-Language", "en-US,en;q=0.5"),
                new KeyValuePair<string, string>("X-Requested-With", "XMLHttpRequest")
            );

            string response = getResponse<string>(message_json);
            var message_data = JsonConvert.DeserializeObject<MailinatorMessage>(response);

            return message_data;
        }

        /// <summary>
        /// Mail interface for "newbie" users
        /// </summary>
        /// <param name="id">Message</param>
        /// <returns>Mail message</returns>
        public Mail getEasyMail(PublicMsg message)
        {
            return getEasyMail(message.id);
        }

        /// <summary>
        /// Mail interface for "newbie" users
        /// </summary>
        /// <param name="id">Message id</param>
        /// <returns>Mail message</returns>
        public Mail getEasyMail(string id)
        {
            var message_data = getMessageData(id);

            return new Mail(
                id,
                message_data.data.fromfull,
                message_data.data.to,
                message_data.data.subject,
                message_data.data.parts.FirstOrDefault().body
            );
        }

        /// <summary>
        /// Get HttpWebRequest response
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="request">Crafted request</param>
        /// <returns>Object<T> response</returns>
        private T getResponse<T>(HttpWebRequest request)
        {
            object obj = null;
            using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                obj = sr.ReadToEnd();
                sr.Close();
            }
            if (typeof(T) == typeof(byte[]))
            {
                obj = Encoding.UTF8.GetBytes((string)obj);
            }
            return (T)obj;
        }

        /// <summary>
        /// Craft a HttpWebRequest request
        /// </summary>
        /// <param name="url">String url</param>
        /// <param name="accept">Accept header</param>
        /// <param name="referer">Referer header</param>
        /// <param name="keep_alive">Keep alive header</param>
        /// <param name="headers">Additional headers</param>
        /// <returns>The request</returns>
        private HttpWebRequest createRequest(string url, string accept = "*/*", string referer = null, bool keep_alive = false, params KeyValuePair<string, string>[] headers)
        {
            while (lastRequest >= DateTime.Now) { }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.CookieContainer = this.cookies;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:49.0) Gecko/20100101 Firefox/49.0";
            request.Accept = accept;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Referer = referer;
            request.Proxy = null;
            request.KeepAlive = keep_alive;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Set(header.Key, header.Value);
                }
            }

            this.lastRequest = DateTime.Now.AddSeconds(1);
            return request;
        }
    }

    #region "Status"
    public class PublicMsg
    {
        public string fromfull { get; set; }
        public string subject { get; set; }
        public string from { get; set; }
        public string origfrom { get; set; }
        public string to { get; set; }
        public string id { get; set; }
        public long time { get; set; }
        public int seconds_ago { get; set; }
    }
    public class MailinatorStatus
    {
        public IList<PublicMsg> public_msgs { get; set; }
        public string enc_public_to { get; set; }
        public string public_to { get; set; }
    }
    #endregion

    #region "Message"
    public class Headers
    {
        public string date { get; set; }
        public string xpriority { get; set; }
        public string subject { get; set; }
        public string importance { get; set; }
        public string messageid { get; set; }
        public string received { get; set; }
        public string from { get; set; }
        public string contenttype { get; set; }
        public string to { get; set; }
        public string errorsto { get; set; }
        public string replyto { get; set; }
    }

    public class Part
    {
        public Headers headers { get; set; }
        public string body { get; set; }
    }

    public class Data
    {
        public string fromfull { get; set; }
        public Headers headers { get; set; }
        public string subject { get; set; }
        public string requestId { get; set; }
        public IList<Part> parts { get; set; }
        public string from { get; set; }
        public string origfrom { get; set; }
        public string to { get; set; }
        public string id { get; set; }
        public long time { get; set; }
        public int seconds_ago { get; set; }
    }

    public class MailinatorMessage
    {
        public Data data { get; set; }
    }
    #endregion

}
