using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Localization;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Localization;

namespace VKClient.Audio
{
    public partial class AlbumsUC : UserControl
    {
        public AllAudioViewModel VM
        {
            get
            {
                return this.DataContext as AllAudioViewModel;
            }
        }

        public ExtendedLongListSelector ListAllAlbums
        {
            get
            {
                return this.AllAlbums;
            }
        }

        public AlbumsUC()
        {
            this.InitializeComponent();
        }

        private void EditAlbumItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || !(frameworkElement.DataContext is AudioAlbumHeader))
                return;
            this.ShowEditAlbum((frameworkElement.DataContext as AudioAlbumHeader).Album);
        }

        private void DeleteAlbumItem_Tap(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null || !(frameworkElement.DataContext is AudioAlbumHeader))
                return;
            AudioAlbumHeader h = frameworkElement.DataContext as AudioAlbumHeader;
            if (MessageBox.Show(CommonResources.GenericConfirmation, AudioResources.DeleteAlbum, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.VM.AllAlbumsVM.DeleteAlbum(h);
        }

        private void AllAlbums_Link_1(object sender, LinkUnlinkEventArgs e)
        {
            (this.DataContext as AllAudioViewModel).AllAlbumsVM.LoadMore(e.ContentPresenter.Content);
        }

        public void ShowEditAlbum(AudioAlbum album)
        {
            DialogService dc = new DialogService();
            dc.SetStatusBarBackground = true;
            dc.HideOnNavigation = false;
            EditAlbumUC editAlbum = new EditAlbumUC();
            editAlbum.textBlockCaption.Text = album.album_id == 0L ? AudioResources.CreateAlbum : AudioResources.EditAlbum;
            editAlbum.textBoxText.Text = album.title ?? "";
            dc.Child = (FrameworkElement)editAlbum;
            editAlbum.buttonSave.Tap += ((s, e) =>
            {
                album.title = editAlbum.textBoxText.Text;
                this.VM.AllAlbumsVM.CreateEditAlbum(album);
                dc.Hide();
            });
            dc.Show(this);
        }
    }
}
