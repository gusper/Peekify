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
        private AppSettingsModel _settings;
        private AppSettingsStore<AppSettingsModel> _settingsStore;

        public AppSettingsController()
        {
            _settings = new AppSettingsModel();
            var directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            var fileName = "spotipeek.settings.xml";
            _settingsStore = new AppSettingsStore<AppSettingsModel>(directoryName, fileName);
        }

        public AppSettingsModel Settings
        {
            get { return _settings; }
        }

        public void Save()
        {
            _settingsStore.Save(_settings);
        }

        public void Load()
        {
            _settings = _settingsStore.Load();
        }
    }
}
