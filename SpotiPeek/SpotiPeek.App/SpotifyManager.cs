using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using SpotifyAPI.Local.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace SpotiPeek.App
{
    class SpotifyManager
    {
        private SpotifyLocalAPI _sApi;

        private static string _spotifyExecutable = @"\spotify\spotify.exe";

        private bool _errorState = false;
        private string _errorStatusText = string.Empty;

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

        public string CurrentTrackInfo
        {
            get
            {
                StatusResponse status;
                string nowPlayingText = "Unknown";
                var attemptsLeft = 3;

                while (attemptsLeft-- > 0)
                {
                    try
                    {
                        status = _sApi.GetStatus();
                        nowPlayingText = string.Format("'{0}' by {1}", status.Track.TrackResource.Name, status.Track.ArtistResource.Name);
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
                            if (!SpotifyLocalAPI.IsSpotifyRunning())
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