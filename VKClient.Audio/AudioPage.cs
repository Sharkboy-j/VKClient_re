using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Audio.Library;
using VKClient.Audio.Localization;
using VKClient.Audio.UserControls;
using VKClient.Audio.ViewModels;
using VKClient.Common;
using VKClient.Common.AudioManager;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Audio
{
    public partial class AudioPage : PageBase
    {
        private bool _isInitialized;
        private AudioPage.PageMode _pageMode;
        private bool _isInMultiSelectMode;
        private long _albumId;
        private ApplicationBar _appBarAudio;
        private ApplicationBar _appBarMultiselect;
        private ApplicationBar _appBarAlbums;
        private ApplicationBarIconButton _appBarButtonAudioPlayer;
        private ApplicationBarIconButton _appBarButtonSearchAudio;
        private ApplicationBarIconButton _appBarButtonEdit;
        private ApplicationBarIconButton _appBarButtonMoveToAlbum;
        private ApplicationBarIconButton _appBarButtonDelete;
        private ApplicationBarIconButton _appBarButtonCancel;
        private ApplicationBarIconButton _appBarButtonAddNewAlbum;
        private DialogService _dialogService;

        private AllAudioViewModel ViewModel
        {
            get
            {
                return this.DataContext as AllAudioViewModel;
            }
        }

        public bool IsInMultiSelectMode
        {
            get
            {
                return this._isInMultiSelectMode;
            }
            set
            {
                if (this._isInMultiSelectMode == value)
                    return;
                this._isInMultiSelectMode = value;
                this.UpdateAppBar();
            }
        }

        public bool HaveAtLeastOneItemSelected
        {
            get
            {
                return false;
            }
        }

        public bool IsOwnAudio
        {
            get
            {
                if (this.CommonParameters.UserOrGroupId == AppGlobalStateManager.Current.LoggedInUserId)
                    return !this.CommonParameters.IsGroup;
                return false;
            }
        }

        public AudioPage()
        {
            this._appBarButtonAudioPlayer = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.nowplaying.rest.png", UriKind.Relative), Text = AudioResources.AppBar_NowPlaying };
            this._appBarButtonSearchAudio = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative), Text = AudioResources.AppBar_Search };
            this._appBarButtonEdit = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.manage.rest.png", UriKind.Relative), Text = AudioResources.Edit };
            this._appBarButtonMoveToAlbum = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.movetofolder.rest.png", UriKind.Relative), Text = AudioResources.AddToAlbum };
            this._appBarButtonDelete = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.delete.rest.png", UriKind.Relative), Text = AudioResources.Delete };
            this._appBarButtonCancel = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative), Text = CommonResources.AppBar_Cancel };
            this._appBarButtonAddNewAlbum = new ApplicationBarIconButton() { IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative), Text = AudioResources.AppBar_Add };
            
            this.InitializeComponent();
            this.BuildAppBar();
            this.ucHeader.OnHeaderTap = new Action(this.HandleHeaderTap);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.allAudio.ListAudios);
            this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.allAlbums.ListAllAlbums);
            this.allAudio.ListAudios.OnRefresh = (Action)(() => this.ViewModel.AllTracks.LoadData(true, false, (Action<BackendResult<List<AudioObj>, ResultCode>>)null, false));
            this.allAlbums.ListAllAlbums.OnRefresh = (Action)(() => this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(true, false, (Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>>)null, false));
            this.allAudio.AllAudios.SelectionChanged += (SelectionChangedEventHandler)((s, e) => this.HandleAudioSelectionChanged((object)this.allAudio.AllAudios, this.allAudio.AllAudios.SelectedItem, false));
            this.allAlbums.AllAlbums.SelectionChanged += new SelectionChangedEventHandler(this.AllAlbums_SelectionChanged);
        }

        private void HandleHeaderTap()
        {
            if (this.mainPivot.SelectedItem == this.pivotItemAudio)
            {
                this.allAudio.ListAudios.ScrollToTop();
            }
            else
            {
                if (this.mainPivot.SelectedItem != this.pivotItemAlbums)
                    return;
                this.allAlbums.ListAllAlbums.ScrollToTop();
            }
        }

        private void AllAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
            if (longListSelector != null)
            {
                AudioAlbumHeader audioAlbumHeader = longListSelector.SelectedItem as AudioAlbumHeader;
                if (audioAlbumHeader != null)
                {
                    if (this._pageMode == AudioPage.PageMode.PickAlbum)
                    {
                        ParametersRepository.SetParameterForId("PickedAlbum", (object)audioAlbumHeader.Album);
                        Navigator.Current.GoBack();
                    }
                    else
                        Navigator.Current.NavigateToAudio((int)this._pageMode, this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, audioAlbumHeader.Album.album_id, 0L, audioAlbumHeader.Album.title);
                }
            }
            longListSelector.SelectedItem = (object)null;
        }

        private void HandleAudioSelectionChanged(object sender, object selectedItem, bool fromSearch)
        {
            ListBox listBox = sender as ListBox;
            ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
            AudioHeader track = selectedItem as AudioHeader;
            if (listBox != null)
                listBox.SelectedItem = (object)null;
            if (longListSelector != null)
                longListSelector.SelectedItem = (object)null;
            if (track == null)
                return;
            if (this._pageMode == AudioPage.PageMode.PickAudio)
            {
                ParametersRepository.SetParameterForId("PickedAudio", (object)track.Track);
                if (this._albumId != 0L)
                    this.NavigationService.RemoveBackEntrySafe();
                Navigator.Current.GoBack();
            }
            else if (listBox != null)
            {
                if (fromSearch)
                    CurrentMediaSource.AudioSource = StatisticsActionSource.search;
                this.NavigateToAudioPlayer(track, listBox.ItemsSource, true);
            }
            else
            {
                if (longListSelector == null)
                    return;
                if (fromSearch)
                    CurrentMediaSource.AudioSource = StatisticsActionSource.search;
                IEnumerable enumerable = !longListSelector.IsFlatList ? this.GetExtendedSelectorGroupedItems(longListSelector.ItemsSource) : (IEnumerable)longListSelector.ItemsSource;
                this.NavigateToAudioPlayer(track, enumerable, true);
            }
        }

        private IEnumerable GetExtendedSelectorGroupedItems(IList itemsSource)
        {
            if (itemsSource != null)
            {
                foreach (IEnumerable enumerable in (IEnumerable)itemsSource)
                {
                    foreach (object obj in enumerable)
                        yield return obj;
                }
            }
        }

        private void NavigateToAudioPlayer(AudioHeader track, IEnumerable enumerable, bool startPlaying = false)
        {
            List<AudioObj> tracks = new List<AudioObj>();
            foreach (object obj in enumerable)
            {
                if (obj is AudioHeader)
                    tracks.Add((obj as AudioHeader).Track);
            }
            PlaylistManager.SetAudioAgentPlaylist(tracks, CurrentMediaSource.AudioSource);
            track.AssignTrack();
            Navigator.Current.NavigateToAudioPlayer(false);
        }

        private void Initialize()
        {
            this._albumId = 0L;
            long exludeAlbumId = 0;
            if (this.NavigationContext.QueryString.ContainsKey("AlbumId"))
                this._albumId = long.Parse(this.NavigationContext.QueryString["AlbumId"]);
            this._pageMode = (AudioPage.PageMode)int.Parse(this.NavigationContext.QueryString["PageMode"]);
            if (this.NavigationContext.QueryString.ContainsKey("ExcludeAlbumId"))
                exludeAlbumId = long.Parse(this.NavigationContext.QueryString["ExcludeAlbumId"]);
            this.DataContext = (object)new AllAudioViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, (uint)this._pageMode > 0U, this._albumId, exludeAlbumId);
            if (this._albumId != 0L)
            {
                this.mainPivot.Items.Remove((object)this.pivotItemAlbums);
                this.pivotItemAudio.Header = (object)new TextBlock()
                {
                    Text = this.NavigationContext.QueryString["AlbumName"],
                    FontSize = 46.0,
                    FontFamily = new FontFamily("Segoe WP SemiLight")
                };
            }
            if (this._pageMode != AudioPage.PageMode.PickAlbum)
                return;
            this.mainPivot.Items.Remove((object)this.pivotItemAudio);
        }

        public void SetTitle()
        {
            string str = "";
            if (this._albumId != 0L)
                str = AudioResources.ALBUM;
            switch (this._pageMode)
            {
                case AudioPage.PageMode.Default:
                    str = AudioResources.Audio;
                    break;
                case AudioPage.PageMode.PickAudio:
                    str = AudioResources.AUDIO_CHOOSE;
                    break;
                case AudioPage.PageMode.PickAlbum:
                    str = AudioResources.Albums_Chose;
                    break;
            }
            this.ucHeader.TextBlockTitle.Text = str;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (!this.IsInMultiSelectMode)
                return;
            e.Cancel = true;
            this.IsInMultiSelectMode = false;
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                if (this.CommonParameters.UserOrGroupId == 0L)
                    this.CommonParameters.UserOrGroupId = AppGlobalStateManager.Current.LoggedInUserId;
                this.Initialize();
                this.PerformInitialLoad();
                this.allAudio.IsInPickMode = this.CommonParameters.PickMode;
                this.UpdateAppBar();
                this._isInitialized = true;
            }
            this.ProcessInputParameters();
            CurrentMediaSource.AudioSource = this.CommonParameters.IsGroup ? StatisticsActionSource.audios_group : StatisticsActionSource.audios_user;
        }

        private void ProcessInputParameters()
        {
            AudioAlbum pickedAlbum = ParametersRepository.GetParameterForIdAndReset("PickedAlbum") as AudioAlbum;
            if (pickedAlbum == null || !this.IsInMultiSelectMode)
                return;
            List<AudioHeader> headersToMove = this.GetSelectedAudioHeaders();
            this.IsInMultiSelectMode = false;
            this.ViewModel.MoveTracksToAlbum(headersToMove, pickedAlbum, (Action<bool>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (!result)
                {
                    ExtendedMessageBox.ShowSafe(CommonResources.GenericErrorText);
                }
                else
                {
                    if (MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(headersToMove.Count, AudioResources.OneAudioMovedFrm, AudioResources.TwoFourAudiosMovedFrm, AudioResources.FiveAudiosMovedFrm, true, pickedAlbum.title, false), AudioResources.MoveAudios, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                        return;
                    Navigator.Current.NavigateToAudio(0, this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, pickedAlbum.album_id, 0L, pickedAlbum.title);
                }
            }))));
        }

        private void PerformInitialLoad()
        {
            switch (this._pageMode)
            {
                case AudioPage.PageMode.Default:
                case AudioPage.PageMode.PickAudio:
                    this.ViewModel.AllTracks.LoadData(false, false, (Action<BackendResult<List<AudioObj>, ResultCode>>)null, false);
                    break;
                case AudioPage.PageMode.PickAlbum:
                    this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(false, false, (Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>>)null, false);
                    break;
            }
        }

        private void AllAudios_MultiSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateAppBar();
        }

        private void UpdateAppBar()
        {
            if (this._dialogService != null && this._dialogService.IsOpen)
                return;
            if (this.mainPivot.SelectedItem == this.pivotItemAudio)
            {
                if (this.IsInMultiSelectMode)
                {
                    this.ApplicationBar = (IApplicationBar)this._appBarMultiselect;
                    this._appBarButtonMoveToAlbum.IsEnabled = this.HaveAtLeastOneItemSelected;
                    this._appBarButtonDelete.IsEnabled = this.HaveAtLeastOneItemSelected;
                }
                else
                {
                    this.ApplicationBar = (IApplicationBar)this._appBarAudio;
                    if (this.IsOwnAudio)
                        return;
                    this._appBarAudio.Buttons.Remove((object)this._appBarButtonEdit);
                }
            }
            else
            {
                if (this.mainPivot.SelectedItem != this.pivotItemAlbums || this._appBarAlbums.Buttons.Count <= 0 && this._appBarAlbums.MenuItems.Count <= 0)
                    return;
                this.ApplicationBar = (IApplicationBar)this._appBarAlbums;
                if (this.IsOwnAudio)
                    return;
                this._appBarAlbums.Buttons.Remove((object)this._appBarButtonAddNewAlbum);
            }
        }

        private void BuildAppBar()
        {
            this._appBarAudio = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._appBarAudio.Opacity = 0.9;
            this._appBarButtonSearchAudio.Click += new EventHandler(this.searchAudio_Click);
            this._appBarAudio.Buttons.Add((object)this._appBarButtonSearchAudio);
            this._appBarButtonEdit.Click += new EventHandler(this._appBarButtonEdit_Click);
            this._appBarMultiselect = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._appBarMultiselect.Opacity = 0.9;
            this._appBarButtonMoveToAlbum.Click += new EventHandler(this._appBarButtonMoveToAlbum_Click);
            this._appBarMultiselect.Buttons.Add((object)this._appBarButtonMoveToAlbum);
            this._appBarButtonDelete.Click += new EventHandler(this._appBarButtonDelete_Click);
            this._appBarMultiselect.Buttons.Add((object)this._appBarButtonDelete);
            this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
            this._appBarMultiselect.Buttons.Add((object)this._appBarButtonCancel);
            this._appBarAlbums = new ApplicationBar()
            {
                BackgroundColor = VKConstants.AppBarBGColor,
                ForegroundColor = VKConstants.AppBarFGColor
            };
            this._appBarAlbums.Opacity = 0.9;
            this._appBarButtonAudioPlayer.Click += new EventHandler(this._appBarButtonAudioPlayer_Click);
        }

        private void _appBarButtonAddNewAlbum_Click(object sender, EventArgs e)
        {
            this.ShowEditAlbum(new AudioAlbum());
        }

        private void ShowEditAlbum(AudioAlbum album)
        {
            DialogService dc = new DialogService();
            dc.SetStatusBarBackground = true;
            dc.HideOnNavigation = false;
            EditAlbumUC editAlbum = new EditAlbumUC();
            editAlbum.textBlockCaption.Text = album.album_id == 0L ? AudioResources.CreateAlbum : AudioResources.EditAlbum;
            dc.Child = (FrameworkElement)editAlbum;
            editAlbum.buttonSave.Tap += ((s, e) =>
            {
                album.title = editAlbum.textBoxText.Text;
                this.ViewModel.AllAlbumsVM.CreateEditAlbum(album);
                dc.Hide();
            });
            dc.Show((UIElement)this.mainPivot);
        }

        private void _appBarButtonAudioPlayer_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToAudioPlayer(false);
        }

        private void _appBarButtonCancel_Click(object sender, EventArgs e)
        {
            this.IsInMultiSelectMode = false;
        }

        private void _appBarButtonDelete_Click(object sender, EventArgs e)
        {
            this.allAudio.DeleteAudios(this.GetSelectedAudioHeaders());
        }

        private List<AudioHeader> GetSelectedAudioHeaders()
        {
            return new List<AudioHeader>();
        }

        private void _appBarButtonMoveToAlbum_Click(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToAudio(2, 0, false, 0, this._albumId, "");
        }

        private void _appBarButtonEdit_Click(object sender, EventArgs e)
        {
            this.IsInMultiSelectMode = true;
        }

        private void searchAudio_Click(object sender, EventArgs e)
        {
            this.EnableSearch();
        }

        private void EnableSearch()
        {
            DialogService dialogService = new DialogService();
            dialogService.BackgroundBrush = (Brush)new SolidColorBrush(Colors.Transparent);
            dialogService.AnimationType = DialogService.AnimationTypes.None;
            int num = 0;
            dialogService.HideOnNavigation = num != 0;
            this._dialogService = dialogService;
            AudioSearchDataProvider searchDataProvider = new AudioSearchDataProvider((IEnumerable<AudioHeader>)this.ViewModel.AllTracks.Collection);
            DataTemplate itemTemplate = (DataTemplate)Application.Current.Resources["TrackTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC();
            searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
            searchUC.Initialize<AudioObj, AudioHeader>((ISearchDataProvider<AudioObj, AudioHeader>)searchDataProvider, (Action<object, object>)((listBox, selectedItem) => this.HandleAudioSelectionChanged(listBox, selectedItem, true)), itemTemplate);
            searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler)((s, ev) => this.mainPivot.Visibility = searchUC.SearchTextBox.Text != "" ? Visibility.Collapsed : Visibility.Visible);
            this._dialogService.Child = (FrameworkElement)searchUC;
            this._dialogService.Show((UIElement)this.mainPivot);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdateAppBar();
            if (this.mainPivot.SelectedItem != this.pivotItemAlbums || this.ViewModel.AllAlbumsVM.AllAlbums.IsLoaded)
                return;
            this.ViewModel.AllAlbumsVM.AllAlbums.LoadData(false, false, (Action<BackendResult<ListResponse<AudioAlbum>, ResultCode>>)null, false);
        }


        private enum PageMode
        {
            Default,
            PickAudio,
            PickAlbum,
        }
    }
}
