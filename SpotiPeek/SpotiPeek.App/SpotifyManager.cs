using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net;
using System.Reflection;
using System.Threading;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPI _sApi;

        private static string _spotifyExecutable = @"\spotify\spotify.exe";
        private const string MutexName = "com.zinkolabs.spotipeek";
        private const int MutexTimeout = 750;

        private bool _errorState = false;
        private string _errorStatusText = string.Empty;
        private string _nowPlayingText = string.Empty;
        private BitmapSource _nowPlayingImage = null;

        public event EventHandler TrackChanged;
        public event EventHandler PlayStateChanged;
        public event EventHandler ErrorStateChanged;

        public SpotifyManager()
        {
            ConnectToLocalSpotifyClient();
        }

        public bool IsInErrorState
        {
            get { return _errorState; }
        }

        public string ErrorStatusText
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
            string nowPlayingText = "Unknown";
            BitmapSource nowPlayingImage = null;

            var attemptsLeft = 3;

            while (attemptsLeft-- > 0)
            {
                try
                {
                    status = _sApi.GetStatus();
                    nowPlayingText = string.Format("'{0}' by {1}", status.Track.TrackResource.Name, status.Track.ArtistResource.Name);

                    // Download image file directly
                    var imageUrl = status.Track.GetAlbumArtUrl(AlbumArtSize.Size160);
                    nowPlayingImage = GetAlbumArtImage(imageUrl);

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
                            ? "Error getting Spotify track information." 
                            : "Spotify is not running.");
                        break;
                    }
                }
            }

            _nowPlayingText = nowPlayingText;
            _nowPlayingImage = nowPlayingImage;
        }

        public static bool IsSpotifyInstalled()
        {
            string pathToSpotify = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _spotifyExecutable;
            Debug.WriteLine(pathToSpotify);
            return File.Exists(pathToSpotify);
        }

        private BitmapSource GetAlbumArtImage(string url)
        {
            using (var wc = new WebClient())
            {
                using (var mutex = new Mutex(false, MutexName))
                {
                    if (mutex.WaitOne(MutexTimeout))
                    {
                        wc.DownloadFile(url, "image.jpg");
                    }
                }
            }

            var fullPathToImage = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "image.jpg");
            return new BitmapImage(new Uri(fullPathToImage, UriKind.Absolute));
        }

        private bool ConnectToLocalSpotifyClient()
        {
            _sApi = new SpotifyLocalAPI();

            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                ReportErrorStateChange(true, "Spotify is not running");
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
            if (PlayStateChanged != null)
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