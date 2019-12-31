using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet.Model
{
    internal class InsertActivity : UserActivity
    {
        public InsertActivity() : this(null) { }

        public InsertActivity(string Username) : base(Username)
        {
            ActivityType = ActivityType.Insert;
        }
    }
}
