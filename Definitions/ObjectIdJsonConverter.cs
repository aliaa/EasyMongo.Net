using MongoDB.Bson;
using Newtonsoft.Json;
using System;

namespace EasyMongoNet
{
    /// <summary>
    /// Use this as parameter to JsonConverter(..) attribute to make able Newtonsoft.Json library to serialize ObjectId types to json.
    /// </summary>
    public class ObjectIdJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((ObjectId)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ObjectId.Parse((string)reader.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ObjectId) == objectType;
        }
    }
}