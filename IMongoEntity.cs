using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
