using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyMongoNet
{
    /// <summary>
    /// Main interface to interact with mongodb. Use this interface in all your interactions with database instead of directly inherited classes like <see cref="MongoDbContext"/>.
    /// </summary>
    public interface IDbContext : IReadOnlyDbContext
    {
        void Save<T>(T item) where T : IMongoEntity;
        DeleteResult DeleteOne<T>(T item) where T : IMongoEntity;
        DeleteResult DeleteOne<T>(ObjectId id) where T : IMongoEntity;
        DeleteResult DeleteOne<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity;
        DeleteResult DeleteMany<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity;
        UpdateResult UpdateOne<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDef, UpdateOptions options = null) where T : IMongoEntity;
        UpdateResult UpdateMany<T>(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity;
        UpdateResult UpdateMany<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity;
        void InsertMany<T>(IEnumerable<T> items, InsertManyOptions options = null) where T : IMongoEntity;
        Task InsertManyAsync<T>(IEnumerable<T> items, InsertManyOptions options = null) where T : IMongoEntity;
        Func<string> GetUserNameFunc { get; set; }
    }
}
