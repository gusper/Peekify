using System;
using System.Windows;
using System.Windows.Input;

namespace SpotiPeek.App
{
    public partial class MainWindow : Window
    {
        private SpotifyManager _sm;
        private App _app = (App)Application.Current;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            RestoreStateFromSettings();

            EnsureSpotifyBetaIsInstalled();

            _sm = new SpotifyManager();
            HookUpEventHandlers();
            CheckAndRespondToErrorState();
            RefreshContent();
        }

        private void RestoreStateFromSettings()
        {
            RestoreStartupWindowLocation();
        }

        private void RestoreStartupWindowLocation()
        {
            Left = _app.Settings.WindowLocationLeft;
            Top = _app.Settings.WindowLocationTop;
        }

        private void EnsureSpotifyBetaIsInstalled()
        {
            if (!SpotifyManager.IsSpotifyBetaInstalled())
            {
                MessageBox.Show("Spotify Beta must be installed to run SpotiPeek", "Can't find Spotify Beta");
                Environment.Exit(-1);
            }
        }

        private void HookUpEventHandlers()
        {
            MouseLeftButtonDown += OnAfterDragWindow;
            TrackInfoLabel.MouseUp += OnAnyMouseButtonUp;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void OnAfterDragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            SaveStateToSettings();
        }

        private void SaveStateToSettings()
        {
            _app.Settings.WindowLocationLeft = (int)Left;
            _app.Settings.WindowLocationTop = (int)Top;
            _app.SaveSettings();
        }

        private void OnErrorStateChanged(object sender, EventArgs e)
        {
            CheckAndRespondToErrorState();
        }

        private void OnPlayStateChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void OnTrackChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void OnAnyMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            Dispatcher.Invoke(() =>
            {
                TrackInfoLabel.Content = _sm.CurrentTrackInfo;
            });
        }

        private void CheckAndRespondToErrorState()
        {
            if (_sm.IsInErrorState)
            {
                // Show error information
                StatusStackPanel.Visibility = Visibility.Visible;
                TrackInfoLabel.Visibility = Visibility.Collapsed;
                StatusInfoLabel.Content = _sm.ErrorStatusText;
            }
            else
            {
                // Show track information
                StatusStackPanel.Visibility = Visibility.Collapsed;
                TrackInfoLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
