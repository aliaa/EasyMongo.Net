
namespace EasyMongoNet.Model
{
    internal class DeleteActivity : UserActivity
    {
        public DeleteActivity() : this(null) { }

        public DeleteActivity(string Username) : base(Username)
        {
            ActivityType = ActivityType.Delete;
        }

        public IMongoEntity DeletedObj { get; set; }
    }
}
