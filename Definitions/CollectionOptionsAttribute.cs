using System;

namespace EasyMongoNet
{
    /// <summary>
    /// Set options for corresponding collection of your model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class CollectionOptionsAttribute : Attribute
    {
        /// <summary>
        /// Set true to ignore automatic collection creation.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Set special name of collection if you don't want to be same as model name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Set a collection as a capped collection.
        /// For more information see: https://docs.mongodb.com/manual/core/capped-collections/
        /// </summary>
        public bool Capped { get; set; } = false;

        /// <summary>
        /// Set maximum size of collection in bytes if you set it as Capped collection.
        /// </summary>
        public long MaxSize { get; set; } = 0;

        /// <summary>
        /// Set maximum number of documents if you set it as Capped collection.
        /// </summary>
        public long MaxDocuments { get; set; } = 0;
    }
}
