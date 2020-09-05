using MongoDB.Driver;

namespace EasyMongoNet
{
    public class MongoConnectionSettings
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
