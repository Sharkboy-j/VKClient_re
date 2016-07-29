using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Posts;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using Windows.Storage.Pickers;

namespace VKClient.Common
{
  public class DocumentsPickerPage : PageBase
  {
    private bool _isInitialized;
    private ObservableCollection<DocumentsSectionViewModel> _sections;
    private ListPickerUC2 _picker;
    internal Grid gridRoot;
    internal GenericHeaderUC ucHeader;
    internal Grid gridContent;
    internal ExtendedLongListSelector list;
    internal Rectangle rectSeparator;
    internal PullToRefreshUC pullToRefresh;
    private bool _contentLoaded;

    private DocumentsViewModel ViewModel
    {
      get
      {
        return this.DataContext as DocumentsViewModel;
      }
    }

    public DocumentsPickerPage()
    {
      this.InitializeComponent();
      this.SuppressMenu = true;
      this.ucHeader.OnHeaderTap += (Action) (() => this.list.ScrollToTop());
      this.list.OnRefresh = (Action) (() => this.ViewModel.CurrentSection.Items.LoadData(true, false, (Action<BackendResult<DocumentsInfo, ResultCode>>) null, false));
      this.pullToRefresh.TrackListBox((ISupportPullToRefresh) this.list);
      this.BuilAppBar();
    }

    private void BuilAppBar()
    {
      this.ApplicationBar = (IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton()
      {
        IconUri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative),
        Text = CommonResources.FriendsPage_AppBar_Search
      };
      applicationBarIconButton.Click += new EventHandler(this.AppBarSearchButton_OnClicked);
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
        DocumentsViewModel parentPageViewModel = new DocumentsViewModel(loggedInUserId);
        DocumentsSectionViewModel sectionViewModel = new DocumentsSectionViewModel(parentPageViewModel, loggedInUserId, 0L, CommonResources.AllDocuments, false, true)
        {
          IsSelected = true
        };
        parentPageViewModel.Sections.Add(sectionViewModel);
        parentPageViewModel.LoadSection(0);
        this.DataContext = (object) parentPageViewModel;
        this._isInitialized = true;
      }
      if (e.NavigationMode != NavigationMode.Back || !ParametersRepository.Contains("FilePicked") && !ParametersRepository.Contains("PickedPhotoDocument"))
        return;
      this.SkipNextNavigationParametersRepositoryClearing = true;
      Navigator.Current.GoBack();
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      DocumentHeader documentHeader = ((LongListSelector) sender).SelectedItem as DocumentHeader;
      if (documentHeader == null)
        return;
      this.list.SelectedItem = null;
      ParametersRepository.SetParameterForId("PickedDocument", (object) documentHeader.Document);
      Navigator.Current.GoBack();
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.CurrentSection.Items.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void AppBarSearchButton_OnClicked(object sender, EventArgs e)
    {
      DocumentsSearchDataProvider searchDataProvider = new DocumentsSearchDataProvider((IEnumerable<DocumentHeader>) this.ViewModel.CurrentSection.Items.Collection);
      DataTemplate dataTemplate = (DataTemplate) this.Resources["ItemTemplate"];
      Action<object, object> selectedItemCallback = (Action<object, object>) ((p, f) => this.List_OnSelectionChanged(p, (SelectionChangedEventArgs) null));
      Action<string> textChangedCallback = (Action<string>) (searchString => this.list.Visibility = searchString != "" ? Visibility.Collapsed : Visibility.Visible);
      DataTemplate itemTemplate = dataTemplate;
      Thickness? margin = new Thickness?(new Thickness(0.0, 77.0, 0.0, 0.0));
      DialogService popup = GenericSearchUC.CreatePopup<Doc, DocumentHeader>((ISearchDataProvider<Doc, DocumentHeader>)searchDataProvider, selectedItemCallback, textChangedCallback, itemTemplate, null, margin);
      EventHandler eventHandler = (EventHandler) ((o, args) => this.list.Visibility = Visibility.Visible);
      popup.Closing += eventHandler;
      Grid grid = this.gridContent;
      popup.Show((UIElement) grid);
    }

    private void FirstButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.NavigateToPhotoPickerPhotos(1, false, true);
    }

    private void SecondButton_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FileOpenPicker fileOpenPicker = new FileOpenPicker();
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["FilePickedType"] = (object) 10;
      foreach (string supportedDocExtension in VKConstants.SupportedDocExtensions)
        fileOpenPicker.FileTypeFilter.Add(supportedDocExtension);
      ((IDictionary<string, object>) fileOpenPicker.ContinuationData)["Operation"] = (object) "DocumentFromPhone";
      fileOpenPicker.PickSingleFileAndContinue();
    }

    private void DocumentsSectionFilter_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.OpenSectionsPicker();
    }

    private void OpenSectionsPicker()
    {
      this._sections = this.ViewModel.Sections;
      this._picker = new ListPickerUC2()
      {
        ItemsSource = (IList) this._sections,
        PickerMaxWidth = 408.0,
        PickerMaxHeight = 768.0,
        BackgroundColor = (Brush) Application.Current.Resources["PhoneCardOverlayBrush"],
        PickerMargin = new Thickness(0.0, 0.0, 0.0, 64.0),
        ItemTemplate = (DataTemplate) this.Resources["FilterItemTemplate"]
      };
      this._picker.ItemTapped += (EventHandler<object>) ((sender, item) =>
      {
        DocumentsSectionViewModel section = item as DocumentsSectionViewModel;
        if (section == null)
          return;
        this.SelectSection(section);
        this.ViewModel.CurrentSection = section;
      });
      Point point = this.rectSeparator.TransformToVisual((UIElement) this.gridRoot).Transform(new Point(0.0, 0.0));
      int num1 = this._sections.IndexOf(this._sections.FirstOrDefault<DocumentsSectionViewModel>((Func<DocumentsSectionViewModel, bool>) (section => section.IsSelected)));
      double num2 = 0.0;
      if (num1 > -1)
        num2 = (double) (num1 * 64);
      this._picker.Show(new Point(8.0, Math.Max(32.0, point.Y - num2)), (FrameworkElement) FramePageUtils.CurrentPage);
    }

    private void SelectSection(DocumentsSectionViewModel section)
    {
      if (this._sections == null || section == null)
        return;
      foreach (DocumentsSectionViewModel section1 in (Collection<DocumentsSectionViewModel>) this._sections)
      {
        int num = section1.SectionId == section.SectionId ? 1 : 0;
        section1.IsSelected = num != 0;
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/DocumentsPickerPage.xaml", UriKind.Relative));
      this.gridRoot = (Grid) this.FindName("gridRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.gridContent = (Grid) this.FindName("gridContent");
      this.list = (ExtendedLongListSelector) this.FindName("list");
      this.rectSeparator = (Rectangle) this.FindName("rectSeparator");
      this.pullToRefresh = (PullToRefreshUC) this.FindName("pullToRefresh");
    }
  }
}
