using SpotifyAPI.SpotifyLocalAPI;
using System;
using System.Threading;
using WinForms = System.Windows.Forms;
using System.Threading.Tasks;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPIClass _sApi;
        private SpotifyEventHandler _sEvents;
        private SpotifyMusicHandler _sMusic;

        private bool _errorState = false;
        private string _errorStatusText = string.Empty;
        private WinForms.Timer _timer;

        public event EventHandler TrackChanged;
        public event EventHandler PlayStateChanged;
        public event EventHandler ErrorStateChanged;

        public SpotifyManager()
        {
            ConnectToLocalSpotifyClient();
            InitializeConnectionErrorTimer();
        }

        public bool IsInErrorState
        {
            get { return _errorState; }
        }

        public string ErrorStatusText
        {
            get { return _errorStatusText; }
        }

        public string CurrentTrackInfo
        {
            get
            {
                Track track;
                string nowPlayingText = "Unknown";

                try
                {
                    _sApi.Update();
                    track = _sMusic.GetCurrentTrack();
                    nowPlayingText = string.Format("'{0}' by {1}", track.GetTrackName(), track.GetArtistName());
                    ReportErrorStateChange(false);
                }
                catch (NullReferenceException)
                {
                    ReportErrorStateChange(true, "Error getting Spotify track information");
                    ConnectToLocalSpotifyClient();
                }

                return nowPlayingText;
            }
        }

        public void ReconnectToSpotify()
        {
            if (_errorState)
            {
                ConnectToLocalSpotifyClient();
            }
        }

        private void InitializeConnectionErrorTimer()
        {
            _timer = new WinForms.Timer();
            _timer.Interval = 20000;
            _timer.Tick += ConnectionErrorRepairHandler;
            _timer.Enabled = true;
            //_timer.Start();
        }

        private void ConnectionErrorRepairHandler(object sender, EventArgs e)
        {
            if (!_errorState)
            {
                return;
            }

            ConnectToLocalSpotifyClient();
        }

        private void ConnectToLocalSpotifyClient()
        {
            _sApi = new SpotifyLocalAPIClass();

            if (!SpotifyLocalAPIClass.IsSpotifyRunning())
            {
                ReportErrorStateChange(true, "Spotify is not running");
                return;
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                _sApi.RunSpotifyWebHelper();
    
                if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
                {
                    ReportErrorStateChange(true, "Failed to launch Spotify Web Helper");
                    return;
                }
            }

            if (!_sApi.Connect())
            {
                ReportErrorStateChange(true, "Failed to connect to Spotify");
            }
            else
            {
                _sMusic = _sApi.GetMusicHandler();

                _sEvents = _sApi.GetEventHandler();
                _sEvents.OnTrackChange += OnTrackChanged;
                _sEvents.OnPlayStateChange += OnPlayStateChanged;
                _sEvents.ListenForEvents(true);

                ReportErrorStateChange(false);
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

        private void OnPlayStateChanged(PlayStateEventArgs e)
        {
            if (PlayStateChanged != null)
            {
                PlayStateChanged.Invoke(this, new EventArgs());
            }
        }

        private void OnTrackChanged(TrackChangeEventArgs e)
        {
            if (TrackChanged != null)
            {
                TrackChanged.Invoke(this, new EventArgs());
            }
        }
    }
}