using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace EasyMongoNet
{
    public enum MongoIndexType
    {
        Acsending,
        Descending,
        Geo2D,
        Geo2DSphere,
        Text,
        Hashed
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CollectionIndexAttribute : Attribute
    {
        private string[] Fields;
        private MongoIndexType[] Types;

        public bool Unique { get; set; } = false;
        public bool Sparse { get; set; } = false;
        public long ExpireAfterSeconds { get; set; } = 0;

        public CollectionIndexAttribute(string[] Fields, MongoIndexType[] Types = null)
        {
            if (Fields == null || Fields.Length == 0)
                throw new Exception();
            this.Fields = Fields;
            this.Types = Types;
        }

        public CollectionIndexAttribute(string IndexDefinition)
        {
            BsonDocument doc = BsonDocument.Parse(IndexDefinition);
            List<string> fieldsList = new List<string>(doc.ElementCount);
            List<MongoIndexType> typesList = new List<MongoIndexType>(doc.ElementCount);
            foreach (var elem in doc.Elements)
            {
                fieldsList.Add(elem.Name);
                if (elem.Value.IsInt32)
                {
                    if (elem.Value.AsInt32 == -1)
                        typesList.Add(MongoIndexType.Descending);
                    else
                        typesList.Add(MongoIndexType.Acsending);
                }
                else
                {
                    switch (elem.Value.AsString.ToLower())
                    {
                        case "2d":
                            typesList.Add(MongoIndexType.Geo2D);
                            break;
                        case "2dsphere":
                            typesList.Add(MongoIndexType.Geo2DSphere);
                            break;
                        case "text":
                            typesList.Add(MongoIndexType.Text);
                            break;
                        case "hashed":
                            typesList.Add(MongoIndexType.Hashed);
                            break;
                        default:
                            throw new Exception("unkonwn index type!");
                    }
                }
            }
            Fields = fieldsList.ToArray();
            Types = typesList.ToArray();
        }

        public IndexKeysDefinition<T> GetIndexKeysDefinition<T>()
        {
            if (Fields.Length == 1)
                return GetIndexDefForOne<T>(Fields[0], Types != null && Types.Length > 0 ? Types[0] : MongoIndexType.Acsending);

            List<IndexKeysDefinition<T>> list = new List<IndexKeysDefinition<T>>(Fields.Length);
            for (int i = 0; i < Fields.Length; i++)
                list.Add(GetIndexDefForOne<T>(Fields[i], Types != null && Fields.Length > i ? Types[i] : MongoIndexType.Acsending));
            return Builders<T>.IndexKeys.Combine(list);
        }

        private static IndexKeysDefinition<T> GetIndexDefForOne<T>(string field, MongoIndexType type)
        {
            switch (type)
            {
                case MongoIndexType.Acsending:
                    return Builders<T>.IndexKeys.Ascending(field);
                case MongoIndexType.Descending:
                    return Builders<T>.IndexKeys.Descending(field);
                case MongoIndexType.Geo2D:
                    return Builders<T>.IndexKeys.Geo2D(field);
                case MongoIndexType.Geo2DSphere:
                    return Builders<T>.IndexKeys.Geo2DSphere(field);
                case MongoIndexType.Text:
                    return Builders<T>.IndexKeys.Text(field);
                case MongoIndexType.Hashed:
                    return Builders<T>.IndexKeys.Hashed(field);
                default:
                    throw new Exception();
            }
        }
    }
}