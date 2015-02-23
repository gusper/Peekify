﻿using System;
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

            EnsureSpotifyBetaIsInstalled();

            _sm = new SpotifyManager();
            HookUpEventHandlers();
            CheckAndRespondToErrorState();
            RefreshContent();
        }

        private void EnsureSpotifyBetaIsInstalled()
        {
            if (!SpotifyManager.IsSpotifyBetaInstalled())
            {
                MessageBox.Show("Spotify Beta must be installed to run SpotiPeek", "Can't find Spotify Beta");
                Environment.Exit(-1);
            }
        }

        private void HookUpEventHandlers()
        {
            this.MouseLeftButtonDown += delegate { DragMove(); };
            TrackInfoLabel.MouseUp += TrackInfoLabel_MouseUp;

            _sm.TrackChanged += OnTrackChanged;
            _sm.PlayStateChanged += OnPlayStateChanged;
            _sm.ErrorStateChanged += OnErrorStateChanged;
        }

        private void OnErrorStateChanged(object sender, EventArgs e)
        {
            CheckAndRespondToErrorState();
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
                // Show error information
                StatusStackPanel.Visibility = Visibility.Visible;
                TrackInfoLabel.Visibility = Visibility.Collapsed;
                StatusInfoLabel.Content = _sm.ErrorStatusText;
            }
            else
            {
                // Show track information
                StatusStackPanel.Visibility = Visibility.Collapsed;
                TrackInfoLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
