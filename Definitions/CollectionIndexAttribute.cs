using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace EasyMongoNet
{
    public enum MongoIndexType
    {
        Ascending,
        Descending,
        Geo2D,
        Geo2DSphere,
        Text,
        Hashed
    }

    /// <summary>
    /// Add this attribute to your model classes on every index you want to have on correspading collection on MongoDB.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CollectionIndexAttribute : Attribute
    {
        /// <summary>
        /// Fields to index. Usually one field should be indexed. But if you want an index with 2 or more fields, add them.
        /// </summary>
        public string[] Fields { get; set; }

        /// <summary>
        /// Index types for selected Fields. You can ignore types if they are "Ascending".
        /// </summary>
        public MongoIndexType[] Types { get; set; }

        /// <summary>
        /// Mark true if the index should be unique.
        /// </summary>
        public bool Unique { get; set; } = false;

        /// <summary>
        /// Mark true if the index should be sparse.
        /// For more information see: https://docs.mongodb.com/manual/core/index-sparse/
        /// </summary>
        public bool Sparse { get; set; } = false;

        /// <summary>
        /// Define document expiration TTL.
        /// For more information see: https://docs.mongodb.com/manual/core/index-ttl/
        /// </summary>
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
                        typesList.Add(MongoIndexType.Ascending);
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

        

        
    }
}