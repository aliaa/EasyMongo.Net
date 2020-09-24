using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace EasyMongoNet
{
    public static class MongoStartupExtentions
    {
        /// <summary>
        /// Finds classes that implements <see cref="EasyMongoNet.IMongoEntity"/> and then injects IMongoCollection&lt;Type&gt; to <paramref name="services"/>.
        /// You should call this method as an extention to your <see cref="IServiceCollection"/> instance on your startup code.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembliesToSearch">List of assemblies to search entity types.</param>
        /// <param name="defaultConnection">Default mongodb connection settings</param>
        /// <param name="customConnections">Alternative mongodb connection settings for specific types.</param>
        public static void FindModelsAndAddMongoCollections(this IServiceCollection services, Assembly[] assembliesToSearch, 
            MongoConnectionSettings defaultConnection, MongoConnectionSettings[] customConnections = null)
        {
            var types = assembliesToSearch.SelectMany(asm => asm.GetTypes()).Where(t => t.GetInterfaces().Contains(typeof(IMongoEntity)));
            FindModelsAndAddMongoCollections(services, types, defaultConnection, customConnections);
        }

        /// <summary>
        /// Injects given entity types as IMongoCollection&lt;Type&gt; to <paramref name="services"/>.
        /// You should call this method as an extention to your <see cref="IServiceCollection"/> instance on your startup code.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="entityTypes">Your entity types that represents a mongodb collection. These types must implement <see cref="EasyMongoNet.IMongoEntity"/></param>
        /// <param name="defaultConnection">Default mongodb connection settings</param>
        /// <param name="customConnections">Alternative mongodb connection settings for specific types.</param>
        public static void FindModelsAndAddMongoCollections(this IServiceCollection services, IEnumerable<Type> entityTypes,
            MongoConnectionSettings defaultConnection, MongoConnectionSettings[] customConnections = null)
        {
            var customConnectionsDic = new Dictionary<string, MongoConnectionSettings>();
            if (customConnections != null)
            {
                foreach (var con in customConnections)
                    if (con.ConnectionString == null)
                        con.ConnectionString = defaultConnection.ConnectionString;
                customConnectionsDic = customConnections.ToDictionary(k => k.Type);
            }
            var databasesDic = new Dictionary<MongoClient, IMongoDatabase>();
            foreach (var type in entityTypes)
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
                SetIndexes(col, type);
                AddCollectionToServices(services, col, type);
            }
            BsonSerializer.RegisterSerializationProvider(new CustomSerializationProvider());
        }


        private static object GetCollecion(IMongoDatabase db, Type type)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(type, typeof(CollectionOptionsAttribute));
            string collectionName = attr?.Name ?? type.Name;
            if (attr != null && !CheckCollectionExists(db, collectionName))
            {
                CreateCollectionOptions options = new CreateCollectionOptions();
                if (attr.Capped)
                {
                    options.Capped = attr.Capped;
                    if (attr.MaxSize > 0)
                        options.MaxSize = attr.MaxSize;
                    if (attr.MaxDocuments > 0)
                        options.MaxDocuments = attr.MaxDocuments;
                }
                db.CreateCollection(collectionName, options);
            }

            var getCollectionMethod = typeof(IMongoDatabase).GetMethod(nameof(IMongoDatabase.GetCollection));
            var genericMethod = getCollectionMethod.MakeGenericMethod(type);
            return genericMethod.Invoke(db, new object[] { collectionName, null });
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

        private static void SetIndexes(object collection, Type type)
        {
            foreach (var attr in type.GetCustomAttributes<CollectionIndexAttribute>())
            {
                var options = new CreateIndexOptions { Sparse = attr.Sparse, Unique = attr.Unique };
                if (attr.ExpireAfterSeconds > 0)
                    options.ExpireAfter = new TimeSpan(attr.ExpireAfterSeconds * 10000000);

                var getIndexKeysMethod = typeof(MongoStartupExtentions).GetMethod(nameof(GetIndexKeysDefinition), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(type);
                var indexKeysDef = getIndexKeysMethod.Invoke(null, new object[] { attr });

                var model = typeof(CreateIndexModel<>).MakeGenericType(type).GetConstructors()[0]
                    .Invoke(new object[] { indexKeysDef, null });

                var indexesProp = collection.GetType().GetProperty(nameof(IMongoCollection<object>.Indexes));
                var indexManager = indexesProp.GetValue(collection);
                var createIndexMethod = indexManager.GetType().GetMethod(nameof(IMongoIndexManager<object>.CreateOne), 
                    new Type[] { model.GetType(), typeof(CreateOneIndexOptions), typeof(CancellationToken) });
                createIndexMethod.Invoke(indexManager, new object[] { model, null, null });
            }
        }

        private static IndexKeysDefinition<T> GetIndexKeysDefinition<T>(CollectionIndexAttribute attr)
        {
            if (attr.Fields.Length == 1)
                return GetIndexDefForOne<T>(attr.Fields[0], attr.Types != null && attr.Types.Length > 0 ? attr.Types[0] : MongoIndexType.Ascending);

            List<IndexKeysDefinition<T>> list = new List<IndexKeysDefinition<T>>(attr.Fields.Length);
            for (int i = 0; i < attr.Fields.Length; i++)
                list.Add(GetIndexDefForOne<T>(attr.Fields[i], attr.Types != null && attr.Fields.Length > i ? attr.Types[i] : MongoIndexType.Ascending));
            return Builders<T>.IndexKeys.Combine(list);
        }

        private static IndexKeysDefinition<T> GetIndexDefForOne<T>(string field, MongoIndexType type)
        {
            switch (type)
            {
                case MongoIndexType.Ascending:
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
