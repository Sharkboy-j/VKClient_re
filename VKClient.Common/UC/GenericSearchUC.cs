using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GenericSearchUC : UserControl
  {
    public GenericSearchViewModelBase ViewModel;
    private Action<object, object> _selectedItemCallback;
    private Action<string> _textChangedCallback;
    private Func<SearchParamsUCBase> _createParamsUCFunc;
    private DialogService _searchParamsDialogService;
    internal Grid LayoutRoot;
    internal TextBox searchTextBox;
    internal TextBlock textBlockWatermarkText;
    internal ExtendedLongListSelector searchResultsListBox;
    private bool _contentLoaded;

    public TextBox SearchTextBox
    {
      get
      {
        return this.searchTextBox;
      }
    }

    public Grid LayoutRootGrid
    {
      get
      {
        return this.LayoutRoot;
      }
    }

    public GenericSearchUC()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.GenericSearchUC_Loaded);
    }

    private void GenericSearchUC_Loaded(object sender, RoutedEventArgs e)
    {
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() => this.searchTextBox.Focus()));
    }

    private ApplicationBar GetSearchParamsAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton(new Uri("/Resources/search_refine.png", UriKind.Relative))
      {
        Text = CommonResources.AppBar_SearchFilter
      };
      applicationBarIconButton.Click += new EventHandler(this.SetFiltersButton_OnClick);
      applicationBar.Buttons.Add((object) applicationBarIconButton);
      return applicationBar;
    }

    private ApplicationBar GetSearchParamsUCAppBar(SearchParamsUCBase paramsUC)
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton(new Uri("/Resources/check.png", UriKind.Relative))
      {
        Text = CommonResources.AppBarMenu_Save
      };
      applicationBarIconButton.Click += (EventHandler) ((sender, args) =>
      {
        if (this._searchParamsDialogService != null && this._searchParamsDialogService.IsOpen)
          this._searchParamsDialogService.Hide();
        Dictionary<string, string> parameters = paramsUC.GetParameters();
        if (parameters != null && parameters.ContainsKey("offset"))
          parameters["offset"] = "0";
        this.ViewModel.Parameters = parameters;
        this.ViewModel.LoadData(true, false, true, false, true);
      });
      applicationBar.Buttons.Add((object) applicationBarIconButton);
      return applicationBar;
    }

    public static DialogService CreatePopup<B, T>(ISearchDataProvider<B, T> searchDataProvider, Action<object, object> selectedItemCallback, Action<string> textChangedCallback, DataTemplate itemTemplate, Func<SearchParamsUCBase> createParamsUCFunc = null, Thickness? margin = null) where B : class where T : class, ISearchableItemHeader<B>
    {
      DialogService dialogService1 = new DialogService();
      dialogService1.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      int num1 = 0;
      dialogService1.HideOnNavigation = num1 != 0;
      int num2 = 1;
      dialogService1.HasPopup = num2 != 0;
      int num3 = 6;
      dialogService1.AnimationType = (DialogService.AnimationTypes) num3;
      DialogService dialogService2 = dialogService1;
      GenericSearchUC genericSearchUc = new GenericSearchUC();
      genericSearchUc.LayoutRootGrid.Margin = margin ?? new Thickness();
      genericSearchUc._textChangedCallback = textChangedCallback;
      genericSearchUc.Initialize<B, T>(searchDataProvider, selectedItemCallback, itemTemplate);
      genericSearchUc._createParamsUCFunc = createParamsUCFunc;
      if (createParamsUCFunc != null)
        dialogService2.AppBar = genericSearchUc.GetSearchParamsAppBar();
      dialogService2.Child = (FrameworkElement) genericSearchUc;
      return dialogService2;
    }

    private void SetFiltersButton_OnClick(object sender, EventArgs e)
    {
      this.LoseTextBoxFocus();
      SearchParamsUCBase paramsUC = this._createParamsUCFunc();
      paramsUC.Initialize(this.ViewModel.Parameters);
      DialogService dialogService = new DialogService();
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      dialogService.AnimationTypeChild = DialogService.AnimationTypes.SlideInversed;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      ApplicationBar searchParamsUcAppBar = this.GetSearchParamsUCAppBar(paramsUC);
      dialogService.AppBar = searchParamsUcAppBar;
      SearchParamsUCBase searchParamsUcBase = paramsUC;
      dialogService.Child = (FrameworkElement) searchParamsUcBase;
      this._searchParamsDialogService = dialogService;
      this._searchParamsDialogService.Show(null);
    }

    private void LoseTextBoxFocus()
    {
      this.searchTextBox.IsTabStop = false;
      this.searchTextBox.IsEnabled = false;
      this.searchTextBox.IsEnabled = true;
      this.searchTextBox.IsTabStop = true;
    }

    public void Initialize<B, T>(ISearchDataProvider<B, T> searchDataProvider, Action<object, object> selectedItemCallback, DataTemplate itemTemplate) where B : class where T : class, ISearchableItemHeader<B>
    {
      GenericSearchViewModel<B, T> genericSearchViewModel = new GenericSearchViewModel<B, T>(searchDataProvider);
      this.DataContext = (object) genericSearchViewModel.SearchVM;
      this.ViewModel = (GenericSearchViewModelBase) genericSearchViewModel;
      this._selectedItemCallback = selectedItemCallback;
      this.searchResultsListBox.ItemTemplate = itemTemplate;
    }

    private void SearchResultsListBox_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      this.searchResultsListBox.Focus();
    }

    private void SearchTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.ViewModel.SearchString = this.searchTextBox.Text;
      if (this.searchTextBox.Text != "")
      {
        this.textBlockWatermarkText.Visibility = Visibility.Collapsed;
        this.VerticalAlignment = VerticalAlignment.Stretch;
        this.searchResultsListBox.Visibility = Visibility.Visible;
      }
      else
      {
        this.textBlockWatermarkText.Visibility = Visibility.Visible;
        this.VerticalAlignment = VerticalAlignment.Top;
        this.searchResultsListBox.Visibility = Visibility.Collapsed;
      }
      Action<string> action = this._textChangedCallback;
      if (action == null)
        return;
      string text = this.searchTextBox.Text;
      action(text);
    }

    private void SearchTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter || !(this.searchTextBox.Text != string.Empty))
        return;
      this.searchResultsListBox.Focus();
    }

    private void SearchTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.searchTextBox.SelectAll();
    }

    private void SearchResultsListBox_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void SearchResultsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      object selectedItem = this.searchResultsListBox.SelectedItem;
      if (selectedItem == null)
        return;
      Action<object, object> action = this._selectedItemCallback;
      if (action != null)
      {
        ExtendedLongListSelector longListSelector = this.searchResultsListBox;
        object obj = selectedItem;
        action((object) longListSelector, obj);
      }
      this.searchResultsListBox.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GenericSearchUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.searchTextBox = (TextBox) this.FindName("searchTextBox");
      this.textBlockWatermarkText = (TextBlock) this.FindName("textBlockWatermarkText");
      this.searchResultsListBox = (ExtendedLongListSelector) this.FindName("searchResultsListBox");
    }
  }
}
