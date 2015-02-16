using System;
using System.Windows;
using System.Windows.Input;

namespace SpotiPeek.App
{
    public partial class MainWindow : Window
    {
        private SpotifyManager _sm;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _sm = new SpotifyManager();
            HookUpEventHandlers();
            CheckAndRespondToErrorState();
            RefreshContent();
        }

        private void HookUpEventHandlers()
        {
            this.MouseLeftButtonDown += delegate { DragMove(); };
            TrackInfoLabel.MouseUp += TrackInfoLabel_MouseUp;
            ReconnectButton.Click += ReconnectButton_Click;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void OnErrorStateChanged(object sender, EventArgs e)
        {
            CheckAndRespondToErrorState();
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e)
        {
            _sm.ReconnectToSpotify();
            RefreshContent();
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
            }));
        }

        private void CheckAndRespondToErrorState()
        {
            if (_sm.IsInErrorState)
            {
                // Show reconnect/refresh button
                StatusStackPanel.Visibility = Visibility.Visible;
                TrackInfoLabel.Visibility = Visibility.Collapsed;
                StatusInfoLabel.Content = _sm.ErrorStatusText;
            }
            else
            {
                // Hide the reconnect/refresh button
                StatusStackPanel.Visibility = Visibility.Collapsed;
                TrackInfoLabel.Visibility = Visibility.Visible;
            }
        }

    }
}
