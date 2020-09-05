using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyMongoNet
{
    public static class MongoStartupExtentions
    {
        public static void FindModelsAndAddMongoCollections(this IServiceCollection services, Assembly[] assembliesToSearch, 
            MongoConnectionSettings defaultConnection, MongoConnectionSettings[] customConnections = null)
        {
            var customConnectionsDic = new Dictionary<string, MongoConnectionSettings>();
            if (customConnections != null)
                customConnectionsDic = customConnections.ToDictionary(k => k.Type);
            var databasesDic = new Dictionary<MongoClient, IMongoDatabase>();
            foreach (var asm in assembliesToSearch)
            {
                foreach (var type in asm.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IMongoEntity))))
                {
                    MongoConnectionSettings connSettings;
                    if (customConnectionsDic.ContainsKey(type.Name))
                        connSettings = customConnectionsDic[type.Name];
                    else
                        connSettings = defaultConnection;
                    var client = new MongoClient(connSettings.ConnectionSettings);
                    IMongoDatabase db;
                    if (databasesDic.ContainsKey(client))
                        db = databasesDic[client];
                    else
                    {
                        db = client.GetDatabase(connSettings.DBName);
                        databasesDic.Add(client, db);
                    }
                    var col = GetCollecion(db, type);
                    AddCollectionToServices(services, col, type);
                }
            }
        }

        private static object GetCollecion(IMongoDatabase db, Type type)
        {
            var getCollectionMethod = typeof(IMongoDatabase).GetMethod(nameof(IMongoDatabase.GetCollection));
            var genericMethod = getCollectionMethod.MakeGenericMethod(type);
            var colName = GetCollectionName(type);
            return genericMethod.Invoke(db, new object[] { colName, null });
        }

        private static string GetCollectionName(Type type)
        {
            var attrs = (CollectionOptionsAttribute[])type.GetCustomAttributes(typeof(CollectionOptionsAttribute), true);
            if (attrs == null || attrs.Length == 0 || attrs[0].Name == null)
                return type.Name;
            return attrs[0].Name;
        }

        private static void AddCollectionToServices(IServiceCollection services, object col, Type type)
        {
            var colType = typeof(IMongoCollection<>).MakeGenericType(type);
            services.AddSingleton(colType, col);
        }

        private static bool CheckCollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collectionCursor = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collectionCursor.Any();
        }
    }
}
