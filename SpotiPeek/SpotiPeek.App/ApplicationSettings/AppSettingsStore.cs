using System.IO;
using System.Runtime.Serialization;

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

            if (File.Exists(GetPathAndFilename()))
            {
                using (var fs = new FileStream(GetPathAndFilename(), FileMode.Open, FileAccess.Read))
                {
                    var ser = new DataContractSerializer(typeof(TData));
                    data = (TData)ser.ReadObject(fs);
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
