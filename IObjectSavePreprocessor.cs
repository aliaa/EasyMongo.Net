using System;

namespace EasyMongoNet
{
    public interface IObjectSavePreprocessor
    {
        void Preprocess(object obj);
    }
}
