using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.UC.InplaceGifViewer;
using VKClient.Common.Utils;
using VKClient.Photos.Library;
using Windows.ApplicationModel.Activation;
using Windows.Storage;

namespace VKClient.Common
{
    public class DocumentsPage : PageBase
    {
        private readonly ApplicationBarIconButton _appBarAddButton;
        private bool _isInitialized;
        private bool _isSearchNow;
        private int _loadedListsCount;
        internal GenericHeaderUC header;
        internal Pivot pivot;
        internal PullToRefreshUC pullToRefresh;
        private bool _contentLoaded;

        private DocumentsViewModel ViewModel
        {
            get
            {
                return this.DataContext as DocumentsViewModel;
            }
        }

        public DocumentsPage()
        {
            this.InitializeComponent();
            this.ApplicationBar = (IApplicationBar)ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
            this._appBarAddButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative),
                Text = CommonResources.FriendsPage_AppBar_Add
            };
            this._appBarAddButton.Click += new EventHandler(DocumentsPage.AppBarAddButton_OnClicked);
            this.ApplicationBar.Buttons.Add((object)this._appBarAddButton);
            ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
            {
                IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
                Text = CommonResources.FriendsPage_AppBar_Search
            };
            applicationBarIconButton.Click += new EventHandler(this.AppBarSearchButton_OnClicked);
            this.ApplicationBar.Buttons.Add((object)applicationBarIconButton);
        }

        protected override void HandleOnNavigatedTo(NavigationEventArgs e)
        {
            base.HandleOnNavigatedTo(e);
            if (!this._isInitialized)
            {
                long ownerId = 0;
                bool isOwnerCommunityAdmined = false;
                if (this.NavigationContext.QueryString.ContainsKey("OwnerId"))
                    ownerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
                if (this.NavigationContext.QueryString.ContainsKey("IsOwnerCommunityAdmined"))
                    isOwnerCommunityAdmined = bool.Parse(this.NavigationContext.QueryString["IsOwnerCommunityAdmined"]);
                if (ownerId != AppGlobalStateManager.Current.LoggedInUserId && !isOwnerCommunityAdmined)
                    this.ApplicationBar.Buttons.Remove((object)this._appBarAddButton);
                DocumentsViewModel parentPageViewModel = new DocumentsViewModel(ownerId);
                parentPageViewModel.Sections.Add(new DocumentsSectionViewModel(parentPageViewModel, ownerId, 0L, CommonResources.Header_ShowAll, isOwnerCommunityAdmined, false));
                parentPageViewModel.LoadSection(0);
                this.DataContext = (object)parentPageViewModel;
                this._isInitialized = true;
            }
            else if (ParametersRepository.Contains("FilePicked"))
            {
                FileOpenPickerContinuationEventArgs continuationEventArgs = ParametersRepository.GetParameterForIdAndReset("FilePicked") as FileOpenPickerContinuationEventArgs;
                StorageFile storageFile;
                if (continuationEventArgs == null)
                {
                    storageFile = (StorageFile)null;
                }
                else
                {
                    IReadOnlyList<StorageFile> files = continuationEventArgs.Files;
                    storageFile = files != null ? ((IEnumerable<StorageFile>)files).FirstOrDefault<StorageFile>() : (StorageFile)null;
                }
                StorageFile file = storageFile;
                if (file == null)
                    return;
                this.ViewModel.UploadDocument(file);
            }
            else
            {
                if (!ParametersRepository.Contains("PickedPhotoDocument"))
                    return;
                StorageFile file = ParametersRepository.GetParameterForIdAndReset("PickedPhotoDocument") as StorageFile;
                if (file == null)
                    return;
                this.ViewModel.UploadDocument(file);
            }
        }

        private void list_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExtendedLongListSelector longListSelector = (ExtendedLongListSelector)sender;
            DocumentHeader documentHeader1 = longListSelector.SelectedItem as DocumentHeader;
            if (documentHeader1 == null)
                return;
            longListSelector.SelectedItem = null;
            if (!documentHeader1.IsGif)
            {
                Navigator.Current.NavigateToWebUri(documentHeader1.Document.url, true, false);
            }
            else
            {
                InplaceGifViewerUC gifViewer = new InplaceGifViewerUC();
                List<PhotoOrDocument> documents = new List<PhotoOrDocument>();
                int num1 = 0;
                List<DocumentHeader> source = this.ViewModel.Sections[this.pivot.SelectedIndex].Items.Collection.ToList<DocumentHeader>();
                if (this._isSearchNow)
                {
                    ObservableCollection<Group<DocumentHeader>> groupedCollection = ((GenericCollectionViewModel2<VKList<Doc>, DocumentHeader>)longListSelector.DataContext).GroupedCollection;
                    source = new List<DocumentHeader>();
                    if (groupedCollection.Count > 0)
                        source = groupedCollection[0].ToList<DocumentHeader>();
                    if (groupedCollection.Count > 1)
                        source.AddRange((IEnumerable<DocumentHeader>)groupedCollection[1].ToList<DocumentHeader>());
                }
                foreach (DocumentHeader documentHeader2 in source.Where<DocumentHeader>((Func<DocumentHeader, bool>)(document => document.IsGif)))
                {
                    if (documentHeader2 == documentHeader1)
                        num1 = documents.Count;
                    documents.Add(new PhotoOrDocument()
                    {
                        document = documentHeader2.Document
                    });
                }
                Action<int> action = (Action<int>)(i =>
                {
                    if (documents[i].document != null)
                    {
                        InplaceGifViewerViewModel gifViewerViewModel = new InplaceGifViewerViewModel(documents[i].document, true, false, false);
                        gifViewer.VM = gifViewerViewModel;
                        gifViewerViewModel.Play(GifPlayStartType.manual);
                        gifViewer.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        InplaceGifViewerViewModel vm = gifViewer.VM;
                        if (vm != null)
                            vm.Stop();
                        gifViewer.Visibility = Visibility.Collapsed;
                    }
                });
                INavigator current = Navigator.Current;
                int selectedIndex = num1;
                List<PhotoOrDocument> photosOrDocuments = documents;
                int num2 = 0;
                int num3 = 0;
                Func<int, Image> func = (Func<int, Image>)(i => null);
                int num4 = 0;
                InplaceGifViewerUC inplaceGifViewerUc = gifViewer;
                Action<int> setContextOnCurrentViewControl = action;
                int num5 = this.ViewModel.OwnerId == AppGlobalStateManager.Current.LoggedInUserId ? 1 : 0;
                Func<int, Image> getImageByIdFunc = null;//omg_re - так в оригинале

                current.NavigateToImageViewerPhotosOrGifs(selectedIndex, photosOrDocuments, num2 != 0, num3 != 0, getImageByIdFunc, this, num4 != 0, (FrameworkElement)inplaceGifViewerUc, setContextOnCurrentViewControl, (Action<int, bool>)((i, b) => { }), num5 != 0);
            }
        }

        private void list_OnLoaded(object sender, RoutedEventArgs e)
        {
            ExtendedLongListSelector list = (ExtendedLongListSelector)sender;
            int pivotItemIndex = this._loadedListsCount;
            list.Loaded -= new RoutedEventHandler(this.list_OnLoaded);
            this._loadedListsCount = this._loadedListsCount + 1;
            this.header.OnHeaderTap += (Action)(() =>
            {
                if (this.pivot.SelectedIndex != pivotItemIndex)
                    return;
                list.ScrollToTop();
            });
            list.OnRefresh = (Action)(() => this.ViewModel.Sections[this.pivot.SelectedIndex].Items.LoadData(true, false, (Action<BackendResult<DocumentsInfo, ResultCode>>)null, false));
            this.pullToRefresh.TrackListBox((ISupportPullToRefresh)list);
        }

        private void list_OnLinked(object sender, LinkUnlinkEventArgs e)
        {
            this.ViewModel.Sections[this.pivot.SelectedIndex].Items.LoadMoreIfNeeded(e.ContentPresenter.Content);
        }

        private void pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ViewModel.LoadSection(this.pivot.SelectedIndex);
        }

        private static void AppBarAddButton_OnClicked(object sender, EventArgs e)
        {
            DocumentPickerUC.Show();
        }

        private void AppBarSearchButton_OnClicked(object sender, EventArgs e)
        {
            DialogService dialogService = new DialogService();
            dialogService.BackgroundBrush = (Brush)new SolidColorBrush(Colors.Transparent);
            dialogService.AnimationType = DialogService.AnimationTypes.None;
            int num = 0;
            dialogService.HideOnNavigation = num != 0;
            DocumentsSearchDataProvider searchDataProvider = new DocumentsSearchDataProvider((IEnumerable<DocumentHeader>)this.ViewModel.Sections[this.pivot.SelectedIndex].Items.Collection);
            DataTemplate itemTemplate = (DataTemplate)this.Resources["ItemTemplate"];
            GenericSearchUC searchUC = new GenericSearchUC();
            searchUC.LayoutRootGrid.Margin = new Thickness(0.0, 77.0, 0.0, 0.0);
            searchUC.Initialize<Doc, DocumentHeader>((ISearchDataProvider<Doc, DocumentHeader>)searchDataProvider, (Action<object, object>)((p, f) => this.list_OnSelectionChanged(p, (SelectionChangedEventArgs)null)), itemTemplate);
            searchUC.SearchTextBox.TextChanged += (TextChangedEventHandler)((s, ev) => this.pivot.Visibility = searchUC.SearchTextBox.Text != "" ? Visibility.Collapsed : Visibility.Visible);
            EventHandler eventHandler = (EventHandler)((p, f) =>
            {
                this.pivot.Visibility = Visibility.Visible;
                this._isSearchNow = false;
            });
            dialogService.Closed += eventHandler;
            this._isSearchNow = true;
            GenericSearchUC genericSearchUc = searchUC;
            dialogService.Child = (FrameworkElement)genericSearchUc;
            Pivot pivot = this.pivot;
            dialogService.Show((UIElement)pivot);
            this.InitializeAdornerControls();
        }

        private void item_OnDeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            DocumentHeader documentHeader = ((FrameworkElement)sender).DataContext as DocumentHeader;
            if (documentHeader == null || MessageBox.Show(CommonResources.GenericConfirmation, UIStringFormatterHelper.FormatNumberOfSomething(1, CommonResources.Documents_DeleteOneFrm, CommonResources.Documents_DeleteTwoFourFrm, CommonResources.Documents_DeleteFiveFrm, true, (string)null, false), MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                return;
            this.ViewModel.DeleteDocument(documentHeader);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/DocumentsPage.xaml", UriKind.Relative));
            this.header = (GenericHeaderUC)this.FindName("header");
            this.pivot = (Pivot)this.FindName("pivot");
            this.pullToRefresh = (PullToRefreshUC)this.FindName("pullToRefresh");
        }
    }
}
