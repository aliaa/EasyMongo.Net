using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet.Model
{
    [CollectionOptions(Name = "ActivityLogs", Capped = true, MaxSize = 10000000)]
    [CollectionSave(WriteLog = false, Preprocess = false)]
    [BsonKnownTypes(typeof(DeleteActivity), typeof(InsertActivity), typeof(UpdateActivity))]
    internal abstract class UserActivity : MongoEntity
    {
        public string Username { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.String)]
        public ActivityType ActivityType { get; protected set; }

        public string CollectionName { get; set; }

        public ObjectId ObjId { get; set; }

        public UserActivity() { }

        public UserActivity(string Username)
        {
            this.Username = Username;
        }
    }

    public enum ActivityType
    {
        Delete,
        Insert,
        Update
    }
}
