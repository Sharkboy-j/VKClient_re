using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.Social;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common
{
    public class ImageViewerBasePage : PageBase
    {
        private bool _isInitialized;
        private bool _isLoadedAtLeastOnce;
        private string _mode;
        private int _photosCount;
        private int _selectedPhotoIndex;
        private PhotosList _photosList;
        internal Grid LayoutRoot;
        private bool _contentLoaded;

        public ImageViewerBasePage()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(this.ImageViewerBasePage_Loaded);
        }

        private void ImageViewerBasePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this._isLoadedAtLeastOnce)
                return;
            this.InitializeImageViewerDecorator();
            this._isLoadedAtLeastOnce = true;
        }

        private void InitializeImageViewerDecorator()
        {
            this._imageViewerDecorator.CancelBackKeyPress = false;
            this._imageViewerDecorator.imageViewer.AllowVerticalSwipe = false;
            Navigator.Current.NavigateToImageViewer(this._photosCount, 0, this._selectedPhotoIndex, this._photosList.Photos.Select<Photo, long>((Func<Photo, long>)(p => p.pid)).ToList<long>(), this._photosList.Photos.Select<Photo, long>((Func<Photo, long>)(p => p.owner_id)).ToList<long>(), this._photosList.Photos.Select<Photo, string>((Func<Photo, string>)(p => p.access_key)).ToList<string>(), this._photosList.Photos, this._mode, false, false, (Func<int, Image>)(ind => (Image)null), (PageBase)this, false);
        }

        protected override void InitializeProgressIndicator()
        {
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                this._mode = this.NavigationContext.QueryString["ViewerMode"];
                this._photosCount = int.Parse(this.NavigationContext.QueryString["PhotosCount"]);
                this._selectedPhotoIndex = int.Parse(this.NavigationContext.QueryString["SelectedPhotoIndex"]);
                string serStr = this.NavigationContext.QueryString["Photos"];
                this._photosList = new PhotosList();
                CacheManager.TryDeserializeFromString((IBinarySerializable)this._photosList, serStr);
                this._isInitialized = true;
            }
            if (this._imageViewerDecorator.IsShown || !this._isLoadedAtLeastOnce)
                return;
            this.InitializeImageViewerDecorator();
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/ImageViewerBasePage.xaml", UriKind.Relative));
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
        }
    }
}
