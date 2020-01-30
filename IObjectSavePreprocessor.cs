using System;

namespace EasyMongoNet
{
    /// <summary>
    /// Interace declaring a preprocessor class.
    /// </summary>
    public interface IObjectSavePreprocessor
    {
        void Preprocess(object obj);
    }
}
