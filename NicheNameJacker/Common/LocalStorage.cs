using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace NicheNameJacker.Common
{
    public class LocalStorage
    {
        static IsolatedStorageFile GetStore() => IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        public static bool SaveDataToFile(object data, string filename)
        {
            try
            {
                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Create, GetStore()))
                using (var writer = new StreamWriter(stream))
                {
                    var serializer = JsonSerializer.Create();
                    serializer.Serialize(writer, data);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static T GetDataFromFile<T>(string filename) where T : class
        {
            try
            {
                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, GetStore()))
                using (var reader = new StreamReader(stream))
                {
                    var serializer = JsonSerializer.Create();
                    return (T)serializer.Deserialize(reader, typeof(T));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}