using Microsoft.Win32;
using System;
using System.Reflection;
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
        private const double _albumArtDisplayTime = 5 * 1000;
        private const string _winRunRegKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

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
            RespondToStateChange();
            RefreshContent();

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnStateChanged;
        }

        private void InitUI()
        {
            InitializeContextMenus();
            _albumArtTimer.Interval = _albumArtDisplayTime;
            _albumArtTimer.Elapsed += AlbumArtTimer_Elapsed;
            DetailsStackPanel.Visibility = Visibility.Collapsed;

            ContextMenuRefresh.Click += OnContextMenuRefresh;
            ContextMenuShowAlbumArt.Click += OnContextMenuShowAlbumArt;
            ContextMenuTransparent.Click += OnContextMenuTransparent;
            ContextMenuAutoStart.Click += OnContextMenuAutoStart;
            ContextMenuAbout.Click += OnContextMenuAbout;
            ContextMenuExit.Click += OnContextMenuExit;
            MouseLeftButtonDown += OnAfterDragWindow;
            MouseLeftButtonUp += OnSingleClick;
            MouseDoubleClick += OnDoubleClick;
        }

        private void OnContextMenuShowAlbumArt(Object sender, RoutedEventArgs e)
        {
            ShowAlbumArt();
        }

        private void InitializeContextMenus()
        {
            ContextMenuAutoStart.IsChecked = IsAutoRunEnabled();
            ContextMenuTransparent.IsChecked = _app.Settings.Data.TransparentMode;
        }

        private void OnContextMenuAutoStart(Object sender, RoutedEventArgs e)
        {
            SetAutoRunState(!ContextMenuAutoStart.IsChecked);
        }

        private void SetAutoRunState(bool enabled)
        {
            var currentState = IsAutoRunEnabled();

            if (currentState == enabled)
                return;

            var autoRunKey = Registry.CurrentUser.OpenSubKey(_winRunRegKey, true);

            if (enabled)
            {
                autoRunKey.SetValue("Peekify", Environment.CommandLine);
                ContextMenuAutoStart.IsChecked = true;
            }
            else
            {
                autoRunKey.DeleteValue("Peekify");
                ContextMenuAutoStart.IsChecked = false;
            }
        }

        private bool IsAutoRunEnabled()
        {
            var isAutoStartEnabled = false;
            var autoRunKey = Registry.CurrentUser.OpenSubKey(_winRunRegKey, false);

            if (autoRunKey.GetValue("Peekify") != null)
            {
                isAutoStartEnabled = true;
            }

            return isAutoStartEnabled;
        }

        private void OnContextMenuTransparent(Object sender, RoutedEventArgs e)
        {
            _app.Settings.Data.TransparentMode = !_app.Settings.Data.TransparentMode;
            _app.Settings.Save();
            ContextMenuTransparent.IsChecked = _app.Settings.Data.TransparentMode;
            SetTransparencyState(_app.Settings.Data.TransparentMode);
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
            new AboutWindow().ShowDialog();
        }

        private void OnContextMenuExit(object sender, RoutedEventArgs e)
        {
            _app.Shutdown(0);
        }

        private void RestoreStateFromSettings()
        {
            RestoreStartupWindowLocation();
            RestoreTransparencyState();
        }

        private void RestoreTransparencyState()
        {
            SetTransparencyState(_app.Settings.Data.TransparentMode);
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

        private void OnStateChanged(object sender, EventArgs e)
        {
            RespondToStateChange();
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
                AlbumArtImage.Source = track.AlbumArt;
            });
        }

        private void ShowAlbumArt()
        {
            Dispatcher.Invoke(() =>
            {
                SetTransparencyState(false);
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
                SetTransparencyState(true);
                SummaryStackPanel.Visibility = Visibility.Visible;
                DetailsStackPanel.Visibility = Visibility.Collapsed;
            });

            if (_albumArtTimer != null)
            {
                _albumArtTimer.Stop();
            }

            _isAlbumArtVisible = false;
        }

        private void SetTransparencyState(bool isTransparent)
        {
            const double transparent = 0.4;
            const double opaque = 1.0;

            if (_app.Settings.Data.TransparentMode && isTransparent)
            {
                Opacity = transparent;
            }
            else
            {
                Opacity = opaque;
            }
        }

        private void AlbumArtTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HideAlbumArt();
        }

        private void RespondToStateChange()
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
                    RefreshContent();
                });
            }
        }
    }
}
