using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpotiPeek.App.ApplicationSettings
{
    public class AppSettingsController
    {
        private AppSettingsModel _data;
        private AppSettingsStore<AppSettingsModel> _settingsStore;

        public AppSettingsController()
        {
            _data = new AppSettingsModel();
            var directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var machineName = Environment.MachineName.ToLower();
            var fileName = "settings." + machineName + ".xml";
            _settingsStore = new AppSettingsStore<AppSettingsModel>(directoryName, fileName);
        }

        public AppSettingsModel Data
        {
            get { return _data; }
        }

        public void Save()
        {
            _settingsStore.Save(_data);
        }

        public void Load()
        {
            _data = _settingsStore.Load();
        }
    }
}
