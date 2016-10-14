namespace MailAPI
{
    public class Mail
    {
        private string _id;
        private string _from;
        private string _to;
        private string _subject;
        private string _body;

        public string Id
        {
            get { return _id; }
            private set { _id = value; }
        }

        public string From
        {
            get { return _from; }
            private set { _from = value; }
        }

        public string To
        {
            get { return _to; }
            private set { _to = value; }
        }

        public string Subject
        {
            get { return _subject; }
            private set { _subject = value; }
        }

        public string Body
        {
            get { return _body; }
            private set { _body = value; }
        }

        public Mail(string id, string from, string to, string subject, string body)
        {
            this._id = id;
            this._from = from;
            this._to = to;
            this._subject = subject;
            this._body = body;
        }
    }
}
