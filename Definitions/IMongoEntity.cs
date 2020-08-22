using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace EasyMongoNet
{
    /// <summary>
    /// Your models must implement this interface or inherited from <see cref="MongoEntity"/> class.
    /// </summary>
    public interface IMongoEntity
    {
        [BsonId]
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        ObjectId Id { get; set; }
    }
}
