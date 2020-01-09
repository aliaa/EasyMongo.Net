using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet
{
    [Serializable]
    public class MongoEntity : IMongoEntity
    {
        public ObjectId Id { get; set; }
    }
}
