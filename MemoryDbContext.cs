using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace EasyMongoNet
{
    /// <summary>
    /// A memory based implementation of IDbContext. Use this class as your database mock in your tests.
    /// </summary>
    public class MemoryDbContext : IDbContext
    {
        public readonly Dictionary<Type, List<object>> Data = new Dictionary<Type, List<object>>();

        private IEnumerable<T> GetListOfType<T>()
        {
            if (!Data.ContainsKey(typeof(T)))
            {
                var list = new List<object>();
                Data.Add(typeof(T), list);
                return list.Cast<T>();
            }
            return Data[typeof(T)].Cast<T>();
        }

        public IAggregateFluent<T> Aggregate<T>(AggregateOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> All<T>() where T : IMongoEntity => Data[typeof(T)].Cast<T>();

        public bool Any<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity => GetListOfType<T>().Where(filter.Compile()).Count() > 0;

        public IMongoQueryable<T> AsQueryable<T>(AggregateOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public long Count<T>() where T : IMongoEntity => Data[typeof(T)].Count;

        public long Count<T>(Expression<Func<T, bool>> filter, CountOptions options = null) where T : IMongoEntity =>
            GetListOfType<T>().Where(filter.Compile()).Count();

        public long Count<T>(FilterDefinition<T> filter, CountOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public DeleteResult DeleteMany<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            int count = 0;
            foreach (var item in GetListOfType<T>().Where(filter.Compile()).ToList())
            {
                Data[typeof(T)].Remove(item);
                count++;
            }
            return new DeleteResult.Acknowledged(count);
        }

        public DeleteResult DeleteOne<T>(T item) where T : IMongoEntity
        {
            if (Data[typeof(T)].Remove(item))
                return new DeleteResult.Acknowledged(1);
            return new DeleteResult.Acknowledged(0);
        }

        public DeleteResult DeleteOne<T>(ObjectId id) where T : IMongoEntity
        {
            T item = GetListOfType<T>().Where(t => t.Id == id).FirstOrDefault();
            if (item != null)
            {
                Data[typeof(T)].Remove(item);
                return new DeleteResult.Acknowledged(1);
            }
            return new DeleteResult.Acknowledged(0);
        }

        public DeleteResult DeleteOne<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity
        {
            T item = GetListOfType<T>().Where(filter.Compile()).FirstOrDefault();
            if (item != null)
            {
                Data[typeof(T)].Remove(item);
                return new DeleteResult.Acknowledged(1);
            }
            return new DeleteResult.Acknowledged(0);
        }

        public IFindFluent<T, T> Find<T>(Expression<Func<T, bool>> filter, FindOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public IFindFluent<T, T> Find<T>(FilterDefinition<T> filter, FindOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public T FindById<T>(ObjectId id) where T : IMongoEntity => GetListOfType<T>().FirstOrDefault(t => t.Id == id);

        public T FindById<T>(string id) where T : IMongoEntity => FindById<T>(ObjectId.Parse(id));

        public T FindFirst<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity => 
            GetListOfType<T>().FirstOrDefault(filter.Compile());

        public IEnumerable<T> FindGetResults<T>(Expression<Func<T, bool>> filter) where T : IMongoEntity =>
            GetListOfType<T>().Where(filter.Compile());

        public void InsertMany<T>(IEnumerable<T> items, InsertManyOptions options = null) where T : IMongoEntity
        {
            foreach (var item in items)
            {
                Data[typeof(T)].Add(item);
            }
        }

        public Task InsertManyAsync<T>(IEnumerable<T> items, InsertManyOptions options = null) where T : IMongoEntity
        {
            InsertMany(items, options);
            return Task.CompletedTask;
        }

        public void Save<T>(T item) where T : IMongoEntity
        {
            T existing = FindById<T>(item.Id);
            if(existing == null)
                DeleteOne<T>(existing);
            Data[typeof(T)].Add(item);
        }

        public UpdateResult UpdateMany<T>(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public UpdateResult UpdateMany<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, UpdateOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }

        public UpdateResult UpdateOne<T>(Expression<Func<T, bool>> filter, UpdateDefinition<T> updateDef, UpdateOptions options = null) where T : IMongoEntity
        {
            throw new NotImplementedException();
        }
    }
}
