﻿using System;
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
using System.Windows.Shapes;

namespace Peekify.App
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            InitEventHandlers();
        }

        private void InitEventHandlers()
        {
            GusperWebLink.RequestNavigate += OnNavigate;
            GusperTwitterLink.RequestNavigate += OnNavigate;
            ZinkoLabsWebLink.RequestNavigate += OnNavigate;
            ZinkoLabsTwitterLink.RequestNavigate += OnNavigate;
            PeekifyGitHubLink.RequestNavigate += OnNavigate;
            SpotifyApiGitHubLink.RequestNavigate += OnNavigate;
            NewtonsoftJsconGitHubLink.RequestNavigate += OnNavigate;
            AppIconLink.RequestNavigate += OnNavigate;
            AppIconAuthorLink.RequestNavigate += OnNavigate;
        }

        private void OnNavigate(Object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}