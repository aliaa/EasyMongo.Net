using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet
{
    /// <summary>
    /// A simple instace of IMongoEntity. You must inherit your models from this class or the IMongoEntity interface.
    /// </summary>
    [Serializable]
    public class MongoEntity : IMongoEntity
    {
        public ObjectId Id { get; set; }
    }
}
