using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void TrackInfoLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RefreshContent();
        }

        private void RefreshContent()
        {
            TrackInfoLabel.Content = _sm.CurrentTrackInfo;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            RefreshContent();
        }
    }
}
