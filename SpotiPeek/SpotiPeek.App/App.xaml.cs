using System.Windows;
using SpotiPeek.App.ApplicationSettings;

namespace SpotiPeek.App
{
	public partial class App : Application
    {
        private AppSettingsController _settings;
                
        public App()
        {
            _settings = new AppSettingsController();
            _settings.Load();
        }

        public AppSettingsController Settings { get { return _settings; } }

        public void SaveSettings()
        {
            _settings.Save();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _settings.Save();
        }
    }
}
