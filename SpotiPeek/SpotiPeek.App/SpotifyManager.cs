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
            _sEvents.ListenForEvents(true);
        }

        private void OnTrackChanged(TrackChangeEventArgs e)
        {
            TrackChanged.Invoke(this, new EventArgs());
        }

        public string CurrentTrackInfo
        {
            get
            {
                _sApi.Update();
                return string.Format("'{0}' by {1}", _sMusic.GetCurrentTrack().GetTrackName(), _sMusic.GetCurrentTrack().GetArtistName());
            }
        }        
    }
}
