using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyMongoNet
{
    public static class MongoCollectionExtentions
    {
        public static T FindById<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity =>
            collection.Find(x => x.Id == id).FirstOrDefault();

        public static async Task<T> FindByIdAsync<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(x => x.Id == id);
            return await cursor.FirstOrDefaultAsync();
        }

        public static T FindFirst<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity =>
            collection.Find(filter).FirstOrDefault();

        public static async Task<T> FindFirstAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(filter, new FindOptions<T, T> { Limit = 1 });
            return await cursor.FirstOrDefaultAsync();
        }

        public static IEnumerable<T> FindGetResults<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity =>
            collection.Find(filter).ToEnumerable();

        public static async Task<IEnumerable<T>> FindGetResultsAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(filter);
            return cursor.ToEnumerable();
        }

        public static DeleteResult DeleteOne<T>(this IMongoCollection<T> collection, T item) where T : IMongoEntity =>
            collection.DeleteOne(x => x.Id == item.Id);

        public static async Task<DeleteResult> DeleteOneAsync<T>(this IMongoCollection<T> collection, T item) where T : IMongoEntity =>
            await collection.DeleteOneAsync(x => x.Id == item.Id);
        
        public static DeleteResult DeleteOne<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity =>
            collection.DeleteOne(x => x.Id == id);

        public static async Task<DeleteResult> DeleteOneAsync<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity =>
            await collection.DeleteOneAsync(x => x.Id == id);

        public static List<T> All<T>(this IMongoCollection<T> collection) where T : IMongoEntity =>
            collection.Find(FilterDefinition<T>.Empty).ToList();

        public static async Task<List<T>> AllAsync<T>(this IMongoCollection<T> collection) where T : IMongoEntity =>
            await (await collection.FindAsync(FilterDefinition<T>.Empty)).ToListAsync();

        public static bool Any<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity =>
            collection.Find(filter).Project(t => t.Id).FirstOrDefault() != null;

        public static async Task<bool> AnyAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(filter, new FindOptions<T, BsonDocument> { Limit = 1, Projection = Builders<T>.Projection.Include(x => x.Id) });
            return (await cursor.FirstOrDefaultAsync()) != null;
        }

        public static void Save<T>(this IMongoCollection<T> collection, T item) where T : IMongoEntity
        {
            if (item.Id == null)
                collection.InsertOne(item);
            else
                collection.ReplaceOne(t => t.Id == item.Id, item);
        }

        public static async Task SaveAsync<T>(this IMongoCollection<T> collection, T item) where T : IMongoEntity
        {
            if (item.Id == null)
                await collection.InsertOneAsync(item);
            else
                await collection.ReplaceOneAsync(t => t.Id == item.Id, item);
        }
    }
}
