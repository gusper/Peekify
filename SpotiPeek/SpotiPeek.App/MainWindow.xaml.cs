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
        }

        void OnTrackChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void TrackInfoLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            Dispatcher.Invoke((Action)(() => {
                TrackInfoLabel.Content = _sm.CurrentTrackInfo;
            }));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshContent();
        }
    }
}
