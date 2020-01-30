using System;

namespace EasyMongoNet
{
    /// <summary>
    /// Use this attribute if you want to log the changes of documents in your collection or you want to preprocess your documents before saving them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CollectionSaveAttribute : Attribute
    {
        /// <summary>
        /// Enables writing logs mechanism.
        /// </summary>
        public bool WriteLog { get; set; }

        /// <summary>
        /// Enables preprocessing mechanism. You should introduce a Preprocessor class to <see cref="MongoDbContext"/> constructor.
        /// </summary>
        public bool Preprocess { get; set; }
    }
}