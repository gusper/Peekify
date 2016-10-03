using System;
using System.IO;
using System.Reflection;

using Peekify.App;

namespace Peekify.App.ApplicationSettings
{
    public class AppSettingsController
    {
        private AppSettingsModel _data;
        private AppSettingsStore<AppSettingsModel> _settingsStore;

        public AppSettingsController()
        {
            _data = new AppSettingsModel();
            var fileName = "settings.xml";
            var path = Utils.GetAppDataPath();
            _settingsStore = new AppSettingsStore<AppSettingsModel>(path, fileName);
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
