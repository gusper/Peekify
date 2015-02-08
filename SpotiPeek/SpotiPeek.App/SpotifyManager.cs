using System;
using System.Threading;
using System.Windows;
using SpotifyAPI.SpotifyLocalAPI;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPIClass _sApi;
        private SpotifyEventHandler _sEvents;
        private SpotifyMusicHandler _sMusic;

        public event EventHandler TrackChanged;
        public event EventHandler PlayStateChanged;

        public SpotifyManager()
        {
            InitializeSpotifyConnection();
        }

        private void InitializeSpotifyConnection()
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
                MessageBox.Show("Failed to connect to Spotify.", "SpotiPeek", MessageBoxButton.OK);
                Application.Current.Shutdown(-1);
            }

            _sMusic = _sApi.GetMusicHandler();

            _sEvents = _sApi.GetEventHandler();
            _sEvents.OnTrackChange += OnTrackChanged;
            _sEvents.OnPlayStateChange += OnPlayStateChanged;
            _sEvents.ListenForEvents(true);
        }

        void OnPlayStateChanged(PlayStateEventArgs e)
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
                catch (NullReferenceException nre)
                {
                    nowPlayingText = "Spotify error";
                }

                return nowPlayingText;
            }
        }
    }
}
