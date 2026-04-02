using UnityEngine;
using QFramework;

namespace LittleRPG
{
    public interface ISaveUtility : IUtility
    {
        void Save<T>(string key, T data, string fileName);
        T Load<T>(string key, T defaultValue, string fileName);
        bool HasFile(string fileName);
        void DeleteFile(string fileName);
    }

    public class EasySaveUtility : ISaveUtility
    {
        public void Save<T>(string key, T data, string fileName)
        {
            // ES3 的 API，指定存入哪个文件
            ES3.Save(key, data, fileName);
        }

        public T Load<T>(string key, T defaultValue, string fileName)
        {
            return ES3.Load(key, fileName, defaultValue);
        }

        public bool HasFile(string fileName)
        {
            return ES3.FileExists(fileName);
        }

        public void DeleteFile(string fileName)
        {
            if (ES3.FileExists(fileName))
            {
                ES3.DeleteFile(fileName);
            }
        }
    }
}