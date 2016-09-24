using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;

namespace SpotiPeek.App.ApplicationSettings
{
    class AppSettingsStore<TData> where TData : new()
    {
        private string _filePath;
        private string _fileName;

        public AppSettingsStore(string filePath, string fileName)
        {
            _filePath = filePath;
            _fileName = fileName;
        }

        public void Save(TData data)
        {
            using (var fs = new FileStream(GetPathAndFilename(), FileMode.Create, FileAccess.Write))
            {
                var ser = new DataContractSerializer(typeof(TData));
                ser.WriteObject(fs, data);
            }
        }

        public TData Load()
        {
            TData data = new TData();
            var filename = GetPathAndFilename();

            if (File.Exists(filename))
            {
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var ser = new DataContractSerializer(typeof(TData));
                    try
                    {
                        data = (TData)ser.ReadObject(fs);
                    }
                    catch (XmlException)
                    {
                        data = new TData();
                    }
                }
            }

            return data;
        }

        private string GetPathAndFilename()
        {
            return _filePath + @"\" + _fileName;
        }
    }
}
