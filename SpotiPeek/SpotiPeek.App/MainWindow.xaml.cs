using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotiPeek.App
{
    public partial class MainWindow : Window
    {
        private SpotifyManager _sm;
        private App _app = (App)Application.Current;
        private Timer _albumArtTimer = new Timer();

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
            this.Opacity = 0.4;
            _albumArtTimer.Interval = 5000;
            _albumArtTimer.Elapsed += AlbumArtTimer_Elapsed;
            ImageStackPanel.Visibility = Visibility.Collapsed;
        }

        private void EnsureSpotifyIsInstalled()
        {
            if (!SpotifyManager.IsSpotifyInstalled())
            {
                MessageBox.Show("Spotify must be installed to run SpotiPeek", "Can't find Spotify");
                _app.Shutdown(-1);
            }
        }

        private void HookUpEventHandlers()
        {
            ContextMenuExit.Click += ContextMenuExit_Click;
            ContextMenuRefresh.Click += ContextMenuRefresh_Click;
            MouseLeftButtonDown += OnAfterDragWindow;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void ContextMenuRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshContent();
        }

        private void ContextMenuExit_Click(object sender, RoutedEventArgs e)
        {
            _app.Shutdown(0);
        }

        private void RestoreStateFromSettings()
        {
            RestoreStartupWindowLocation();
        }

        private void RestoreStartupWindowLocation()
        {
            Left = Math.Abs(_app.Settings.Data.WindowLocationLeft);
            Top = Math.Abs(_app.Settings.Data.WindowLocationTop);
        }

        private void OnAfterDragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            SaveStateToSettings();
        }

        private void SaveStateToSettings()
        {
            _app.Settings.Data.WindowLocationLeft = (int)Math.Abs(Left);
            _app.Settings.Data.WindowLocationTop = (int)Math.Abs(Top);
            _app.Settings.Save();
        }

        private void OnErrorStateChanged(object sender, EventArgs e)
        {
            CheckAndRespondToErrorState();
        }

        private void OnPlayStateChanged(object sender, EventArgs e)
        {
            RefreshContent();
            ShowAlbumArt();
        }

        private void OnTrackChanged(object sender, EventArgs e)
        {
            RefreshContent();
            ShowAlbumArt();
        }

        private void RefreshContent()
        {
            Dispatcher.Invoke(() =>
            {
                _sm.UpdateStatus();
                TrackInfoLabel.Content = _sm.CurrentTrackInfo;
                AlbumArtImage.Source = _sm.CurrentAlbumImage;
            });
        }

        private void ShowAlbumArt()
        {
            Dispatcher.Invoke(() =>
            {
                this.Opacity = 1.0;
                ImageStackPanel.Visibility = Visibility.Visible;
            });

            _albumArtTimer.Stop();
            _albumArtTimer.Start();
        }

        private void AlbumArtTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.Opacity = 0.4;
                ImageStackPanel.Visibility = Visibility.Collapsed;
            });

            _albumArtTimer.Stop();
        }

        private void CheckAndRespondToErrorState()
        {
            if (_sm.IsInErrorState)
            {
                Dispatcher.Invoke(() =>
                {
                    // Show error information
                    StatusInfoLabel.Visibility = Visibility.Visible;
                    TrackInfoLabel.Visibility = Visibility.Collapsed;
                    StatusInfoLabel.Content = _sm.ErrorStatusText;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    // Show track information
                    StatusInfoLabel.Visibility = Visibility.Collapsed;
                    TrackInfoLabel.Visibility = Visibility.Visible;
                });
            }
        }
    }
}
