using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EasyMongoNet
{
    /// <summary>
    /// Your models must implement this interface or inherited from <see cref="MongoEntity"/> class.
    /// </summary>
    public interface IMongoEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        string Id { get; set; }
    }
}
