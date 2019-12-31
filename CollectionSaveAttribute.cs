using System;

namespace EasyMongoNet
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CollectionSaveAttribute : Attribute
    {
        public bool WriteLog { get; set; }
        public bool Preprocess { get; set; }
    }
}