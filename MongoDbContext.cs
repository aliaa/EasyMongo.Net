using EasyMongoNet.Model;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EasyMongoNet
{
    public class MongoDbContext : IDbContext
    {
        private readonly IObjectSavePreprocessor objectPreprocessor;
        private readonly string connectionString;
        private readonly bool setDictionaryConventionToArrayOfDocuments;
        private readonly List<CustomMongoConnection> customConnections = new List<CustomMongoConnection>();
        private readonly Dictionary<string, object> Collections = new Dictionary<string, object>();
        private readonly string dbName;
        private readonly Func<string> getUsernameFunc;

        private static readonly Dictionary<Type, CollectionSaveAttribute> SaveAttrsCache = new Dictionary<Type, CollectionSaveAttribute>();
        public readonly IMongoDatabase Database;
        public bool DefaultWriteLog { get; set; } = false;
        public bool DefaultPreprocess { get; set; } = false;

        public MongoDbContext(string dbName, string connectionString, 
            Func<string> getUsernameFunc,
            bool setDictionaryConventionToArrayOfDocuments = false, 
            IEnumerable<CustomMongoConnection> customConnections = null, 
            IObjectSavePreprocessor objectPreprocessor = null)
        {
            this.dbName = dbName;
            this.connectionString = connectionString;
            this.getUsernameFunc = getUsernameFunc;
            this.setDictionaryConventionToArrayOfDocuments = setDictionaryConventionToArrayOfDocuments;
            this.Database = GetDatabase(connectionString, dbName, setDictionaryConventionToArrayOfDocuments);
            if (customConnections != null)
                this.customConnections.AddRange(customConnections);
            this.objectPreprocessor = objectPreprocessor;
        }

        private IMongoDatabase GetDatabase(string connectionString, string dbName, bool setDictionaryConventionToArrayOfDocuments)
        {
            MongoClient client = new MongoClient(connectionString);
            var db = client.GetDatabase(dbName);

            if (setDictionaryConventionToArrayOfDocuments)
            {
                ConventionRegistry.Register(
                    nameof(DictionaryRepresentationConvention),
                    new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfDocuments) }, _ => true);
            }
            BsonSerializer.RegisterSerializationProvider(new CustomSerializationProvider());
            return db;
        }

        protected IMongoCollection<T> GetCollection<T>()
        {
            string collectionName = GetCollectionName(typeof(T));
            if (Collections.ContainsKey(collectionName))
                return (IMongoCollection<T>)Collections[collectionName];

            IMongoCollection<T> collection;
            CustomMongoConnection customConnection = customConnections.FirstOrDefault(c => c.Type == collectionName);
            if (customConnection != null)
            {
                collection = GetCollection<T>(GetDatabase(customConnection.ConnectionString, customConnection.DBName, setDictionaryConventionToArrayOfDocuments));
            }
            else
                collection = GetCollection<T>(Database);

            SetIndexes(collection);
            try
            {
                Collections.Add(collectionName, collection);
            }
            catch { }
            return collection;
        }

        private static IMongoCollection<T> GetCollection<T>(IMongoDatabase db)
        {
            CollectionOptionsAttribute attr = (CollectionOptionsAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(CollectionOptionsAttribute));
            string collectionName = attr?.Name ?? typeof(T).Name;
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
            return db.GetCollection<T>(collectionName);
        }

        private static string GetCollectionName(Type type)
        {
            var attrs = (CollectionOptionsAttribute[])type.GetCustomAttributes(typeof(CollectionOptionsAttribute), true);
            if (attrs == null || attrs.Length == 0 || attrs[0].Name == null)
                return type.Name;
            return attrs[0].Name;
        }

        private static bool CheckCollectionExists(IMongoDatabase db, string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collectionCursor = db.ListCollections(new ListCollectionsOptions { Filter = filter });
            return collectionCursor.Any();
        }

        private static void SetIndexes<T>(IMongoCollection<T> collection)
        {
            foreach (CollectionIndexAttribute attr in typeof(T).GetCustomAttributes<CollectionIndexAttribute>())
            {
                var options = new CreateIndexOptions { Sparse = attr.Sparse, Unique = attr.Unique };
                if (attr.ExpireAfterSeconds > 0)
                    options.ExpireAfter = new TimeSpan(attr.ExpireAfterSeconds * 10000000);
                CreateIndexModel<T> model = new CreateIndexModel<T>(attr.GetIndexKeysDefinition<T>(), options);
                collection.Indexes.CreateOne(model);
            }
        }

        private static CollectionSaveAttribute GetSaveAttribute(Type type)
        {
            if (SaveAttrsCache.ContainsKey(type))
                return SaveAttrsCache[type];
            var attr = (CollectionSaveAttribute)Attribute.GetCustomAttribute(type, typeof(CollectionSaveAttribute));
            if (attr != null)
                SaveAttrsCache[type] = attr;
            return attr;
        }

        private bool ShouldWriteLog<T>()
        {
            bool writeLog = DefaultWriteLog;
            CollectionSaveAttribute attr = GetSaveAttribute(typeof(T));
            if (attr != null)
                writeLog = attr.WriteLog;
            return writeLog;
        }

        public T FindById<T>(ObjectId id) where T : IMongoEntity
        {
            return GetCollection<T>().Find(t => t.Id == id).FirstOrDefault();
        }

        public T FindById<T>(string id) where T : IMongoEntity
        {
            return FindById<T>(ObjectId.Parse(id));
        }

        public void Save<T>(T item) where T : IMongoEntity
        {
            bool writeLog = DefaultWriteLog;
            bool preprocess = DefaultPreprocess;
            Type type = typeof(T);
            CollectionSaveAttribute attr = GetSaveAttribute(type);
            if (attr != null)
            {
                writeLog = attr.WriteLog;
                preprocess = attr.Preprocess;
            }

            if (preprocess && objectPreprocessor != null)
                objectPreprocessor.Preprocess(item);

            ActivityType activityType;
            T oldValue = default(T);
            if (item.Id == ObjectId.Empty)
            {
                GetCollection<T>().InsertOne(item);
                activityType = ActivityType.Insert;
            }
            else
            {
                if (writeLog)
                    oldValue = FindById<T>(item.Id);
                GetCollection<T>().ReplaceOne(t => t.Id == item.Id, item, new ReplaceOptions { IsUpsert = true });
                activityType = ActivityType.Update;
            }
            if (writeLog)
            {
                string username = getUsernameFunc();
                if (activityType == ActivityType.Insert)
                {
                    var insertActivity = new InsertActivity(username) { CollectionName = GetCollectionName(type), ObjId = item.Id };
                    Save((UserActivity)insertActivity);
                }
                else
                {
                    UpdateActivity update = new UpdateActivity(username) { CollectionName = GetCollectionName(type), ObjId = oldValue.Id };
                    update.SetDiff(oldValue, item);
                    if (update.Diff.Count > 0)
                        Save((UserActivity)update);
                }
            }
        }

        public DeleteResult DeleteOne<T>(T item) where T : IMongoEntity
        {
            var result = GetCollection<T>().DeleteOne(t => t.Id == item.Id);
            if (ShouldWriteLog<T>())
            {
                var deleteActivity = new DeleteActivity(getUsernameFunc()) { CollectionName = GetCollectionName(typeof(T)), ObjId = item.Id, DeletedObj = item };
                Save((UserActivity)deleteActivity);
            }
            return result;
        }

        public DeleteResult DeleteOne<T>(ObjectId id) where T : IMongoEntity
        {
            if (ShouldWriteLog<T>())
            {
                T item = FindById<T>(id);
                if (item != null)
                {
                    var deleteActivity = new DeleteActivity(getUsernameFunc()) { CollectionName = GetCollectionName(typeof(T)), ObjId = item.Id, DeletedObj = item };
                    Save((UserActivity)deleteActivity);
                }
            }
            return GetCollection<T>().DeleteOne(t => t.Id == id);
        }

        public DeleteResult DeleteOne<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            if (ShouldWriteLog<T>())
            {
                T item = Find(filter).FirstOrDefault();
                if (item != null)
                {
                    var deleteActivity = new DeleteActivity(getUsernameFunc()) { CollectionName = GetCollectionName(typeof(T)), ObjId = item.Id, DeletedObj = item };
                    Save((UserActivity)deleteActivity);
                }
            }
            return GetCollection<T>().DeleteOne(filter);
        }

        public DeleteResult DeleteMany<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            return GetCollection<T>().DeleteMany<T>(filter);
        }

        public UpdateResult UpdateOne<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDef, UpdateOptions options = null) where T : IMongoEntity
        {
            T oldValue = default(T);
            bool writeLog = ShouldWriteLog<T>();
            if (writeLog)
                oldValue = Find(filter).FirstOrDefault();
            var res = GetCollection<T>().UpdateOne(filter, updateDef, options);
            if (oldValue != null)
            {
                UpdateActivity updateActivity = new UpdateActivity(getUsernameFunc()) { CollectionName = GetCollectionName(typeof(T)), ObjId = oldValue.Id };
                T newValue = FindById<T>(oldValue.Id);
                updateActivity.SetDiff(oldValue, newValue);
                Save((UserActivity)updateActivity);
            }
            return res;
        }

        public UpdateResult UpdateMany<T>(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().UpdateMany(filter, update, options);
        }

        public UpdateResult UpdateMany<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().UpdateMany(filter, update, options);
        }

        public IEnumerable<T> All<T>() where T : IMongoEntity
        {
            return GetCollection<T>().Find(FilterDefinition<T>.Empty).ToEnumerable();
        }

        public bool Any<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            return GetCollection<T>().Find(filter).Project(t => t.Id).FirstOrDefault() != ObjectId.Empty;
        }

        public IFindFluent<T, T> Find<T>(Expression<Func<T, bool>> filter, FindOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().Find(filter, options);
        }

        public IFindFluent<T, T> Find<T>(FilterDefinition<T> filter, FindOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().Find(filter, options);
        }

        public long Count<T>() where T : IMongoEntity
        {
            return GetCollection<T>().CountDocuments(_ => true);
        }

        public long Count<T>(Expression<Func<T, bool>> filter, CountOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().CountDocuments(filter, options);
        }

        public long Count<T>(FilterDefinition<T> filter, CountOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().CountDocuments(filter, options);
        }

        public IAggregateFluent<T> Aggregate<T>(AggregateOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().Aggregate(options);
        }

        public IMongoQueryable<T> AsQueryable<T>(AggregateOptions options = null) where T : IMongoEntity
        {
            return GetCollection<T>().AsQueryable(options);
        }
    }
}
