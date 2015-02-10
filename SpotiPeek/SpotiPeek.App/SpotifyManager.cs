using System;
using System.Threading;
using System.Windows;
using SpotifyAPI.SpotifyLocalAPI;
using WinForms = System.Windows.Forms;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPIClass _sApi;
        private SpotifyEventHandler _sEvents;
        private SpotifyMusicHandler _sMusic;

        private bool _errorState = false;
        private WinForms.Timer _timer;

        public event EventHandler TrackChanged;
        public event EventHandler PlayStateChanged;

        public SpotifyManager()
        {
            ConnectToLocalSpotifyClient();
            InitializeConnectionErrorTimer();
        }

        private void InitializeConnectionErrorTimer()
        {
            _timer = new WinForms.Timer();
            _timer.Interval = 20000;
            _timer.Tick += ConnectionErrorRepairHandler;
            _timer.Enabled = true;
            _timer.Start();            
        }

        void ConnectionErrorRepairHandler(object sender, EventArgs e)
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
                _sApi.RunSpotify();
                Thread.Sleep(4000);
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                _sApi.RunSpotifyWebHelper();
                Thread.Sleep(4000);
            }

            if (!_sApi.Connect())
            {
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
                    nowPlayingText = "Spotify connection error";
                    _errorState = true;
                }

                return nowPlayingText;
            }
        }
    }
}