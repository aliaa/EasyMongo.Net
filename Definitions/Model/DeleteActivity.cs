
namespace EasyMongoNet.Model
{
    /// <summary>
    /// Represents a log to deleting job and holds a copy of deleted object.
    /// </summary>
    public class DeleteActivity : UserActivity
    {
        public DeleteActivity() : this(null) { }

        public DeleteActivity(string Username) : base(Username)
        {
            ActivityType = ActivityType.Delete;
        }

        public IMongoEntity DeletedObj { get; set; }
    }
}
