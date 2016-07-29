using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.ViewModels;

namespace VKClient.Common.UC
{
    public class MiniPlayerUC : UserControl
    {
        //private bool _loaded;
        internal Grid LayoutRoot;
        internal StackPanel stackPanelTrackTitle;
        private bool _contentLoaded;

        private AudioPlayerViewModel VM
        {
            get
            {
                return this.DataContext as AudioPlayerViewModel;
            }
        }

        public MiniPlayerUC()
        {
            this.InitializeComponent();
            this.DataContext = (object)new AudioPlayerViewModel();
            this.Loaded += new RoutedEventHandler(this.MiniPlayerUC_Loaded);
        }

        private void MiniPlayerUC_Loaded(object sender, RoutedEventArgs e)
        {
            this.VM.Activate(true);
            //this._loaded = true;
        }

        private void playImage_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Play();
        }

        private void pauseImage_Tap(object sender, GestureEventArgs e)
        {
            this.VM.Pause();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/MiniPlayerUC.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
            this.stackPanelTrackTitle = (StackPanel)this.FindName("stackPanelTrackTitle");
        }
    }
}
