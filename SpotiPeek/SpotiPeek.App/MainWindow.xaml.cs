using System;
using System.Windows;
using System.Windows.Input;

namespace SpotiPeek.App
{
    public partial class MainWindow : Window
    {
        private SpotifyManager _sm = new SpotifyManager();

        public MainWindow()
        {
            InitializeComponent();
            HookUpEventHandlers();
        }

        private void HookUpEventHandlers()
        {
            this.MouseLeftButtonDown += delegate { DragMove(); };
            TrackInfoLabel.MouseUp += TrackInfoLabel_MouseUp;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
        }

        private void OnPlayStateChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void OnTrackChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void TrackInfoLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                TrackInfoLabel.Content = _sm.CurrentTrackInfo;
                CheckAndRespondToErrorState();
            }));
        }

        private void CheckAndRespondToErrorState()
        {
            if (_sm.IsInErrorState)
            {
                // Show reconnect/refresh button
                ReconnectButton.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide the reconnect/refresh button
                ReconnectButton.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshContent();
        }
    }
}
