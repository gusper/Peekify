using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;

namespace SpotiPeek.App
{
	class SpotifyManager
    {
        private SpotifyLocalAPI _sApi;

        private static string _spotifyExecutable = @"\spotify\spotify.exe";
        private const int _pollInterval = 5 * 60 * 1000;

        private Timer _processWatcherTimer;
        private bool _errorState = false;
        private string _errorStatusText = string.Empty;
        private string _nowPlayingText = string.Empty;
        private BitmapSource _nowPlayingImage = null;
        
        internal event EventHandler TrackChanged;
        internal event EventHandler PlayStateChanged;
        internal event EventHandler ErrorStateChanged;

        internal SpotifyManager()
        {
            ConnectToLocalSpotifyClient();
        }

        internal bool IsInErrorState
        {
            get { return _errorState; }
        }

        internal string ErrorStatusText
        {
            get { return _errorStatusText; }
        }

        internal string CurrentTrackInfo
        {
            get { return _nowPlayingText; }
        }

        internal BitmapSource CurrentAlbumImage
        {
            get { return _nowPlayingImage; }
        }

        internal void UpdateStatus()
        {
            StatusResponse status;
            var nowPlayingText = "Unknown";
            BitmapSource nowPlayingImage = null;

            var attemptsLeft = 3;

            while (attemptsLeft-- > 0)
            {
                try
                {
                    status = _sApi.GetStatus();
                    nowPlayingText = $"'{status.Track.TrackResource.Name}' by {status.Track.ArtistResource.Name}";
                    nowPlayingImage = GetAlbumArtImage(status.Track);

                    ReportErrorStateChange(false);
                    break;
                }
                catch (NullReferenceException)
                {
                    if (ConnectToLocalSpotifyClient())
                    {
                        continue;
                    }
                    else
                    {
                        ReportErrorStateChange(true, SpotifyLocalAPI.IsSpotifyRunning() 
                            ? $"Error: {this.ErrorStatusText}" : "Spotify is not running.");
                        break;
                    }
                }
            }

            _nowPlayingText = nowPlayingText;

            if (nowPlayingImage != null)
            {
                _nowPlayingImage = (BitmapImage)nowPlayingImage;
            }
        }

        internal static bool IsSpotifyInstalled()
        {
            string pathToSpotify = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _spotifyExecutable;
            return File.Exists(pathToSpotify);
        }

        private BitmapImage GetAlbumArtImage(Track track)
        {
            var albumArtSize = AlbumArtSize.Size320;
            var url = track.GetAlbumArtUrl(albumArtSize);
            var albumUrlId = GetAlbumIdFromUrl(url);

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(albumUrlId))
            {
                Debug.WriteLine("Album art url or urlid were empty strings.");
                return null;
            }

            var imageFilePath = GetPathToFileInCache(albumArtSize, albumUrlId);

            if (!File.Exists(imageFilePath))
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.DownloadFile(url, imageFilePath);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception swallowed: " + e.Message);
                    return null;
                }
            }

            // Had to use frozen or it would lead to a 'file in use' exception on next download
            return (BitmapImage)new BitmapImage(new Uri(imageFilePath)).GetCurrentValueAsFrozen();
        }

        private string GetAlbumIdFromUrl(string url)
        {
            var slashIndex = url.LastIndexOf('/') + 1;
            var albumUrlId = url.Substring(slashIndex, url.Length - slashIndex);
            return albumUrlId;
        }

        private string GetPathToFileInCache(AlbumArtSize albumArtSize, string albumUrlId)
        {
            var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var cacheDirectory = Path.Combine(appDirectory, "cache");
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }
            var fileName = albumUrlId + "-" + GetSizeAsString(albumArtSize) + ".jpg";
            var imageFilePath = Path.Combine(cacheDirectory, fileName);
            return imageFilePath;
        }

        private string GetSizeAsString(AlbumArtSize albumArtSize)
        {
            switch (albumArtSize)
            {
                case AlbumArtSize.Size160: return "160";
                case AlbumArtSize.Size320: return "320";
                case AlbumArtSize.Size640: return "640";
            }

            return "0";
        }

        private bool ConnectToLocalSpotifyClient()
        {
            _sApi = new SpotifyLocalAPI();

            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                ReportErrorStateChange(true, "Spotify is not running");
                StartSpotifyProcessWatcher();
                return false;
            }

            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                ReportErrorStateChange(true, "Spotify Web Helper not running");
                return false;
            }

            if (!_sApi.Connect())
            {
                ReportErrorStateChange(true, "Failed to connect to Spotify");
                return false;
            }
            else
            {
                _sApi.OnTrackChange += OnTrackChanged;
                _sApi.OnPlayStateChange += OnPlayStateChanged;
                _sApi.ListenForEvents = true;

                ReportErrorStateChange(false);
            }

            return true;
        }

        private void StartSpotifyProcessWatcher()
        {
            _processWatcherTimer = new Timer(CheckForSpotifyProcess, null, _pollInterval, _pollInterval);
        }

        private void CheckForSpotifyProcess(object state)
        {
            var procs = Process.GetProcessesByName("spotify");
            if (procs.Length > 0)
            {
                Debug.WriteLine("Found spotify.exe");
                _processWatcherTimer.Change(Timeout.Infinite, Timeout.Infinite);
                UpdateStatus();
            }
            else
            {
                Debug.WriteLine("Spotify.exe not running");
            }
        }

        private void ReportErrorStateChange(bool isInErrorState, string errorText = "")
        {
            _errorStatusText = errorText;
            _errorState = isInErrorState;

            if (ErrorStateChanged != null)
            {
                ErrorStateChanged.Invoke(this, null);
            }
        }

        private void OnPlayStateChanged(object sender, PlayStateEventArgs e)
        {
            if (PlayStateChanged != null && e.Playing)
            {
                PlayStateChanged.Invoke(this, new EventArgs());
            }
        }

        private void OnTrackChanged(object sender, TrackChangeEventArgs e)
        {
            if (TrackChanged != null)
            {
                TrackChanged.Invoke(this, new EventArgs());
            }
        }
    }
}