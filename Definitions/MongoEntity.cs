using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace EasyMongoNet
{
    /// <summary>
    /// A simple instace of IMongoEntity. You must inherit your models from this class or the IMongoEntity interface.
    /// </summary>
    [Serializable]
    public class MongoEntity : IMongoEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string Id { get; set; }
    }
}
