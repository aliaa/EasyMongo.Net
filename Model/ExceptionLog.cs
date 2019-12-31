using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet.Model
{
    [CollectionSave(WriteLog = false, Preprocess = false)]
    public class ExceptionLog : MongoEntity
    {
        public static implicit operator ExceptionLog(Exception ex)
        {
            return new ExceptionLog(ex);
        }

        private const int MAX_INNER_EXCEPTION = 50;

        public ExceptionLog() { }

        public ExceptionLog(Exception Exception) : this(Exception, null, null, null) { }

        public ExceptionLog(Exception Exception, string Url, string Username, string IP)
        {
            this.Url = Url;
            this.Username = Username;
            this.IP = IP;
            this.Exception = Exception;
            Message = Exception.Message;
            StackTrace = Exception.StackTrace;
            if (Exception.InnerException != null)
            {
                List<Exception> hierarchyList = new List<Exception>();
                hierarchyList.Add(Exception);
                Exception current = Exception.InnerException;
                while (current != null && !hierarchyList.Contains(current) && InnerExceptions.Count < MAX_INNER_EXCEPTION)
                {
                    InnerExceptions.Add(Tuple.Create(current, current.Message, current.StackTrace));
                    hierarchyList.Add(current);
                    current = current.InnerException;
                }
            }
        }

        public Exception Exception { get; private set; }
        public string Url { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Username { get; set; }
        public string IP { get; set; }
        public List<Tuple<Exception, string, string>> InnerExceptions { get; set; } = new List<Tuple<Exception, string, string>>();
        public bool Checked { get; set; }
    }
}
