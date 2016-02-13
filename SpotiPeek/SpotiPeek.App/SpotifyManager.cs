using SpotifyAPI.SpotifyLocalAPI;
using System;
using System.Diagnostics;
using System.IO;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPIClass _sApi;
        private SpotifyEventHandler _sEvents;
        private SpotifyMusicHandler _sMusic;

        private static string _spotifyExecutable;
        private static bool _useSpotifyBeta;

        private bool _errorState = false;
        private string _errorStatusText = string.Empty;

        public event EventHandler TrackChanged;
        public event EventHandler PlayStateChanged;
        public event EventHandler ErrorStateChanged;

        static SpotifyManager()
        {
            _spotifyExecutable = FindSpotifyExe();
        }

        public SpotifyManager()
        {
            ConnectToLocalSpotifyClient();
        }

        private static string FindSpotifyExe()
        {
            var args = Environment.GetCommandLineArgs();

            if (args[0].ToLower() == "beta")
            {
                _useSpotifyBeta = true;
                return @"\spotifybeta\spotifybeta.exe";
            }

            _useSpotifyBeta = false;
            return @"\spotify\spotify.exe";
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
                var attemptsLeft = 3;

                while (attemptsLeft-- > 0)
                {
                    try
                    {
                        _sApi.Update();
                        track = _sMusic.GetCurrentTrack();
                        nowPlayingText = string.Format("'{0}' by {1}", track.GetTrackName(), track.GetArtistName());
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
                            if (!SpotifyLocalAPIClass.IsSpotifyRunning())
                            {
                                ReportErrorStateChange(true, "Spotify is not running.");
                            }
                            else
                            {
                                ReportErrorStateChange(true, "Error getting Spotify track information.");
                            }

                            break;
                        }
                    }
                }

                return nowPlayingText;
            }
        }

        public static bool IsSpotifyInstalled()
        {
            string pathToSpotify = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + _spotifyExecutable;
            Debug.WriteLine(pathToSpotify);
            return File.Exists(pathToSpotify);
        }

        private bool ConnectToLocalSpotifyClient()
        {
            _sApi = new SpotifyLocalAPIClass(_useSpotifyBeta);

            if (!SpotifyLocalAPIClass.IsSpotifyRunning())
            {
                ReportErrorStateChange(true, "Spotify is not running");
                return false;
            }

            if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
            {
                _sApi.RunSpotifyWebHelper();

                if (!SpotifyLocalAPIClass.IsSpotifyWebHelperRunning())
                {
                    ReportErrorStateChange(true, "Failed to launch Spotify Web Helper");
                    return false;
                }
            }

            if (!_sApi.Connect())
            {
                ReportErrorStateChange(true, "Failed to connect to Spotify");
                return false;
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