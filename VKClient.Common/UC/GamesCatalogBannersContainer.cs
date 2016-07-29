using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.Events;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesCatalogBannersContainer : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (List<GameHeader>), typeof (GamesCatalogBannersContainer), new PropertyMetadata(new PropertyChangedCallback(GamesCatalogBannersContainer.OnItemsSourceChanged)));
    internal StackPanel panelCatalogBanners;
    internal GamesCatalogBannersSlideView slideView;
    internal GroupHeaderUC groupHeader;
    private bool _contentLoaded;

    public List<GameHeader> ItemsSource
    {
      get
      {
        return (List<GameHeader>) this.GetValue(GamesCatalogBannersContainer.ItemsSourceProperty);
      }
      set
      {
        this.SetValue(GamesCatalogBannersContainer.ItemsSourceProperty, (object) value);
      }
    }

    public GamesCatalogBannersContainer()
    {
      this.InitializeComponent();
      this.panelCatalogBanners.Visibility = Visibility.Collapsed;
      this.slideView.CreateSingleElement = (Func<Control>) (() => (Control) new CatalogBannerUC());
      this.slideView.NextElementSwipeDelay = TimeSpan.FromSeconds(5.0);
      this.slideView.IsCycled = true;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesCatalogBannersContainer bannersContainer = d as GamesCatalogBannersContainer;
      if (bannersContainer == null)
        return;
      List<GameHeader> gameHeaderList = e.NewValue as List<GameHeader>;
      if (gameHeaderList == null)
      {
        bannersContainer.panelCatalogBanners.Visibility = Visibility.Collapsed;
        bannersContainer.groupHeader.Visibility = Visibility.Visible;
      }
      else
      {
        bannersContainer.groupHeader.Visibility = Visibility.Collapsed;
        bannersContainer.panelCatalogBanners.Visibility = Visibility.Visible;
        bannersContainer.slideView.Items = new ObservableCollection<object>((IEnumerable<object>) gameHeaderList);
      }
    }

    private void BorderCatalog_OnTap(object sender, GestureEventArgs e)
    {
      GameHeader currentGame = this.slideView.GetCurrentGame();
      if (currentGame == null || this.ItemsSource == null)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(this.ItemsSource.Cast<object>().ToList<object>(), GamesClickSource.catalog, "", this.ItemsSource.IndexOf(currentGame), null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GamesCatalogBannersContainer.xaml", UriKind.Relative));
      this.panelCatalogBanners = (StackPanel) this.FindName("panelCatalogBanners");
      this.slideView = (GamesCatalogBannersSlideView) this.FindName("slideView");
      this.groupHeader = (GroupHeaderUC) this.FindName("groupHeader");
    }
  }
}
