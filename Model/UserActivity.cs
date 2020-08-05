using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet.Model
{
    /// <summary>
    /// Base class to logging users activities on changing sensitive data containing 3 subclasses: <see cref="InsertActivity"/>, <see cref="UpdateActivity"/> and <see cref="DeleteActivity"/>.
    /// </summary>
    [CollectionOptions(Name = "ActivityLogs", Capped = true, MaxSize = 10000000)]
    [CollectionSave(WriteLog = false, Preprocess = false)]
    [BsonKnownTypes(typeof(DeleteActivity), typeof(InsertActivity), typeof(UpdateActivity))]
    [CollectionIndex(new string[] { nameof(ObjId) })]
    [CollectionIndex(new string[] { nameof(Username) })]
    public class UserActivity : MongoEntity
    {
        public string Username { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.String)]
        public ActivityType ActivityType { get; set; }

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
