using MongoDB.Bson.Serialization;
using System;

namespace EasyMongoNet
{
    internal class CustomSerializationProvider : IBsonSerializationProvider
    {
        static LocalDateTimeSerializer dateTimeSerializer = new LocalDateTimeSerializer();

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(DateTime))
                return dateTimeSerializer;

            return null; // falls back to Mongo defaults
        }
    }
}
