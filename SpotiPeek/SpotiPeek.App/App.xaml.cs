using SpotiPeek.App.ApplicationSettings;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace SpotiPeek.App
{
    public partial class App : Application
    {
        private AppSettingsController _settingsMgr;
                
        public App()
        {
            _settingsMgr = new AppSettingsController();
            _settingsMgr.Load();
        }

        public AppSettingsModel Settings { get { return _settingsMgr.Settings; } }

        public void SaveSettings()
        {
            _settingsMgr.Save();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _settingsMgr.Save();
        }
    }
}
