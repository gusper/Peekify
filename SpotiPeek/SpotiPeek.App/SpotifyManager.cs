using SpotifyAPI.SpotifyLocalAPI;
using System;
using System.Threading;
using WinForms = System.Windows.Forms;

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
                string nowPlayingText = string.Empty;

                try
                {
                    _sApi.Update();
                    track = _sMusic.GetCurrentTrack();
                    nowPlayingText = string.Format("'{0}' by {1}", track.GetTrackName(), track.GetArtistName());
                }
                catch (NullReferenceException)
                {
                    nowPlayingText = string.Empty;
                    _errorState = true;
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
                _errorStatusText = "Spotify is not running";
                _errorState = true;
                return;

                //_sApi.RunSpotify();
                //Thread.Sleep(4000);

                //if (!SpotifyLocalAPIClass.IsSpotifyRunning())
                //{
                //    _errorStatusText = "Failed to launch Spotify";
                //    _errorState = true;
                //    return;
                //}
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                _sApi.RunSpotifyWebHelper();
                Thread.Sleep(4000);

                if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
                {
                    _errorStatusText = "Failed to launch Spotify Web Helper";
                    _errorState = true;
                    return;
                }
            }

            if (!_sApi.Connect())
            {
                _errorStatusText = "Failed to connect to Spotify";
                _errorState = true;
            }
            else
            {
                _sMusic = _sApi.GetMusicHandler();

                _sEvents = _sApi.GetEventHandler();
                _sEvents.OnTrackChange += OnTrackChanged;
                _sEvents.OnPlayStateChange += OnPlayStateChanged;
                _sEvents.ListenForEvents(true);

                _errorState = false;
            }
        }

        private void OnPlayStateChanged(PlayStateEventArgs e)
        {
            PlayStateChanged.Invoke(this, new EventArgs());
        }

        private void OnTrackChanged(TrackChangeEventArgs e)
        {
            TrackChanged.Invoke(this, new EventArgs());
        }
    }
}