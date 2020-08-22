using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyMongoNet
{
    /// <summary>
    /// Defines alternative connection strings beside the main connection string for special models.
    /// </summary>
    public class CustomMongoConnection
    {
        public string Type { get; set; }
        public string DBName { get; set; }
        public MongoClientSettings ConnectionSettings { get; set; }

        private string _connectionString;
        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                _connectionString = value;
                ConnectionSettings = MongoClientSettings.FromConnectionString(value);
            }
        }
    }
}
