using Microsoft.Phone.Controls;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class RecommendedGroupsPage : PageBase
  {
    private bool _isInitialized;
    //private bool _isCategoryLoaded;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal Pivot pivot;
    internal PivotItem pivotItemRecommendations;
    internal ExtendedLongListSelector recommendationsListBox;
    internal PivotItem pivotItemCatalog;
    internal ExtendedLongListSelector catalogListBox;
    private bool _contentLoaded;

    private RecommendedGroupsViewModel VM
    {
      get
      {
        return this.DataContext as RecommendedGroupsViewModel;
      }
    }

    public RecommendedGroupsPage()
    {
      this.InitializeComponent();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        RecommendedGroupsViewModel recommendedGroupsViewModel = new RecommendedGroupsViewModel(int.Parse(this.NavigationContext.QueryString["CategoryId"]), this.NavigationContext.QueryString["CategoryName"]);
        this.DataContext = (object) recommendedGroupsViewModel;
        recommendedGroupsViewModel.Recommendations.LoadData(false, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false);
        this.UpdateCatalogVisibility();
        recommendedGroupsViewModel.CatalogCategories.Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Collection_CollectionChanged);
        this._isInitialized = true;
      }
      CurrentCommunitySource.Source = CommunityOpenSource.Recommendations;
    }

    private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.UpdateCatalogVisibility();
    }

    private void UpdateCatalogVisibility()
    {
      bool flag = this.VM.CatalogCategories.Collection.Count > 0;
      if (flag && !this.pivot.Items.Contains((object) this.pivotItemCatalog))
      {
        this.pivot.Items.Add((object) this.pivotItemCatalog);
      }
      else
      {
        if (flag || !this.pivot.Items.Contains((object) this.pivotItemCatalog))
          return;
        this.pivot.Items.Remove((object) this.pivotItemCatalog);
      }
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      CatalogCategoryHeader catalogCategoryHeader = (sender as FrameworkElement).DataContext as CatalogCategoryHeader;
      if (catalogCategoryHeader == null)
        return;
      Navigator.Current.NavigateToGroupRecommendations(catalogCategoryHeader.CategoryId, catalogCategoryHeader.Title);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/RecommendedGroupsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.pivot = (Pivot) this.FindName("pivot");
      this.pivotItemRecommendations = (PivotItem) this.FindName("pivotItemRecommendations");
      this.recommendationsListBox = (ExtendedLongListSelector) this.FindName("recommendationsListBox");
      this.pivotItemCatalog = (PivotItem) this.FindName("pivotItemCatalog");
      this.catalogListBox = (ExtendedLongListSelector) this.FindName("catalogListBox");
    }
  }
}
