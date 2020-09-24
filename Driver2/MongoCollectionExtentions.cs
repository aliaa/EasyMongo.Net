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
        public static T FindById<T>(this IMongoCollection<T> collection, ObjectId id) where T : IMongoEntity =>
            collection.Find(x => x.Id == id).FirstOrDefault();

        public static async Task<T> FindByIdAsync<T>(this IMongoCollection<T> collection, ObjectId id) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(x => x.Id == id);
            return await cursor.FirstOrDefaultAsync();
        }

        public static T FindById<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity =>
            FindById(collection, ObjectId.Parse(id));

        public static async Task<T> FindByIdAsync<T>(this IMongoCollection<T> collection, string id) where T : IMongoEntity =>
            await FindByIdAsync(collection, ObjectId.Parse(id));

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
        
        public static DeleteResult DeleteOne<T>(this IMongoCollection<T> collection, ObjectId id) where T : IMongoEntity =>
            collection.DeleteOne(x => x.Id == id);

        public static async Task<DeleteResult> DeleteOneAsync<T>(this IMongoCollection<T> collection, ObjectId id) where T : IMongoEntity =>
            await collection.DeleteOneAsync(x => x.Id == id);

        public static IEnumerable<T> All<T>(this IMongoCollection<T> collection) where T : IMongoEntity =>
            collection.Find(FilterDefinition<T>.Empty).ToEnumerable();

        public static async Task<IEnumerable<T>> AllAsync<T>(this IMongoCollection<T> collection) where T : IMongoEntity =>
            (await collection.FindAsync(FilterDefinition<T>.Empty)).ToEnumerable();

        public static bool Any<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity =>
            collection.Find(filter).Project(t => t.Id).FirstOrDefault() != ObjectId.Empty;

        public static async Task<bool> AnyAsync<T>(this IMongoCollection<T> collection, Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            var cursor = await collection.FindAsync(filter, new FindOptions<T, ObjectId> { Limit = 1, Projection = Builders<T>.Projection.Include(x => x.Id) });
            return (await cursor.FirstOrDefaultAsync()) != ObjectId.Empty;
        }
    }
}
