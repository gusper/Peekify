using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Peekify.App
{
    public partial class MainWindow : Window
    {
        private SpotifyManager _sm;
        private App _app = (App)Application.Current;
        private Timer _albumArtTimer = new Timer();
        private bool _isAlbumArtVisible = false;
        private const double _transparentOpacity = 0.4;
        private const double _solidOpacity = 1.0;
        private const double _albumArtDisplayTime = 5000;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            RestoreStateFromSettings();
            InitSpotifyConnection();
            InitUI();
        }

        private void InitSpotifyConnection()
        {
            EnsureSpotifyIsInstalled();
            _sm = new SpotifyManager();
            CheckAndRespondToErrorState();
            RefreshContent();

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void InitUI()
        {
            Opacity = _transparentOpacity;
            _albumArtTimer.Interval = _albumArtDisplayTime;
            _albumArtTimer.Elapsed += AlbumArtTimer_Elapsed;
            DetailsStackPanel.Visibility = Visibility.Collapsed;

            ContextMenuExit.Click += OnContextMenuExit;
            ContextMenuRefresh.Click += OnContextMenuRefresh;
            ContextMenuAbout.Click += OnContextMenuAbout;
            MouseLeftButtonDown += OnAfterDragWindow;
            MouseLeftButtonUp += OnSingleClick;
            MouseDoubleClick += OnDoubleClick;
        }

        private void EnsureSpotifyIsInstalled()
        {
            if (!SpotifyManager.IsSpotifyInstalled())
            {
                MessageBox.Show("Spotify must be installed to run Peekify", "Can't find Spotify");
                _app.Shutdown(-1);
            }
        }

        private void OnContextMenuRefresh(object sender, RoutedEventArgs e)
        {
            RefreshContent();
        }

        private void OnContextMenuAbout(Object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Peekify\nBy Gus Perez\nZinko Labs LLC\nhttp://zinkolabs.com/peekify", "About...");
        }

        private void OnContextMenuExit(object sender, RoutedEventArgs e)
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
            Top = 0;
        }

        private void OnAfterDragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
            Top = 0;
            SaveStateToSettings();
        }

        private void SaveStateToSettings()
        {
            _app.Settings.Data.WindowLocationLeft = (int)Math.Abs(Left);
            _app.Settings.Save();
        }

        private void OnErrorStateChanged(object sender, EventArgs e)
        {
            CheckAndRespondToErrorState();
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _sm.TogglePlayPauseState();
        }

        private void OnSingleClick(object sender, MouseButtonEventArgs e)
        {
            if (_isAlbumArtVisible)
            {
                HideAlbumArt();
            }
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
                var track = _sm.CurrentTrackInfo;
                TrackInfoLabel.Content = $"'{ track.SongTitle }' by { track.ArtistName }";
                AlbumArtSongLabel.Content = track.SongTitle;
                AlbumArtArtistLabel.Content = track.ArtistName;
                AlbumArtAlbumLabel.Content = track.AlbumTitle;
                AlbumArtImage.Source = _sm.CurrentAlbumImage;
            });
        }

        private void ShowAlbumArt()
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = _solidOpacity;
                SummaryStackPanel.Visibility = Visibility.Collapsed;
                DetailsStackPanel.Visibility = Visibility.Visible;
            });

            _albumArtTimer.Stop();
            _albumArtTimer.Start();

            _isAlbumArtVisible = true;
        }

        private void HideAlbumArt()
        {
            Dispatcher.Invoke(() =>
            {
                Opacity = _transparentOpacity;
                SummaryStackPanel.Visibility = Visibility.Visible;
                DetailsStackPanel.Visibility = Visibility.Collapsed;
            });

            if (_albumArtTimer != null)
            {
                _albumArtTimer.Stop();
            }

            _isAlbumArtVisible = false;
        }

        private void AlbumArtTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HideAlbumArt();
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
