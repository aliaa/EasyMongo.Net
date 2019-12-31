using System;

namespace EasyMongoNet
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class CollectionOptionsAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Capped { get; set; } = false;
        public long MaxSize { get; set; } = 0;
        public long MaxDocuments { get; set; } = 0;
    }
}
