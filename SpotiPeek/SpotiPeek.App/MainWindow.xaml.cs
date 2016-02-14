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

            EnsureSpotifyIsInstalled();

            _sm = new SpotifyManager();
            HookUpEventHandlers();
            CheckAndRespondToErrorState();
            RefreshContent();
        }

        private void EnsureSpotifyIsInstalled()
        {
            if (!SpotifyManager.IsSpotifyInstalled())
            {
                MessageBox.Show("Spotify must be installed to run SpotiPeek", "Can't find Spotify");
                Environment.Exit(-1);
            }
        }

        private void HookUpEventHandlers()
        {
            ContextMenuExit.Click += ContextMenuExit_Click;
            MouseLeftButtonDown += OnAfterDragWindow;
            MainPane.MouseUp += OnAnyMouseButtonUp;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void ContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void RestoreStateFromSettings()
        {
            RestoreStartupWindowLocation();
        }

        private void RestoreStartupWindowLocation()
        {
            Left = Math.Abs(_app.Settings.WindowLocationLeft);
            Top = Math.Abs(_app.Settings.WindowLocationTop);
        }

        private void OnAfterDragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            SaveStateToSettings();
        }

        private void SaveStateToSettings()
        {
            _app.Settings.WindowLocationLeft = (int)Math.Abs(Left);
            _app.Settings.WindowLocationTop = (int)Math.Abs(Top);
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
