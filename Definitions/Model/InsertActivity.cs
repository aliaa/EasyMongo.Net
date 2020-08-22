using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet.Model
{
    /// <summary>
    /// Represents a log to insertion job.
    /// </summary>
    public class InsertActivity : UserActivity
    {
        public InsertActivity() : this(null) { }

        public InsertActivity(string Username) : base(Username)
        {
            ActivityType = ActivityType.Insert;
        }
    }
}
