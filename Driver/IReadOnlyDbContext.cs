using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EasyMongoNet
{
    public interface IReadOnlyDbContext
    {
        T FindById<T>(ObjectId id) where T : IMongoEntity;
        T FindById<T>(string id) where T : IMongoEntity;
        IEnumerable<T> All<T>() where T : IMongoEntity;
        bool Any<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity;
        IFindFluent<T, T> Find<T>(Expression<Func<T, bool>> filter, FindOptions options = null) where T : IMongoEntity;
        IFindFluent<T, T> Find<T>(FilterDefinition<T> filter, FindOptions options = null) where T : IMongoEntity;
        T FindFirst<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity;
        IEnumerable<T> FindGetResults<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity;
        long Count<T>() where T : IMongoEntity;
        long Count<T>(Expression<Func<T, bool>> filter, CountOptions options = null) where T : IMongoEntity;
        long Count<T>(FilterDefinition<T> filter, CountOptions options = null) where T : IMongoEntity;
        IAggregateFluent<T> Aggregate<T>(AggregateOptions options = null) where T : IMongoEntity;
        IMongoQueryable<T> AsQueryable<T>(AggregateOptions options = null) where T : IMongoEntity;
    }
}
