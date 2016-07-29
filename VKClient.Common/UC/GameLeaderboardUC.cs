using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;

namespace VKClient.Common.UC
{
  public class GameLeaderboardUC : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (List<GameLeaderboardItemHeader>), typeof (GameLeaderboardUC), new PropertyMetadata(new PropertyChangedCallback(GameLeaderboardUC.OnItemsSourceChanged)));
    internal ItemsControl itemsControl;
    private bool _contentLoaded;

    public List<GameLeaderboardItemHeader> ItemsSource
    {
      get
      {
        return (List<GameLeaderboardItemHeader>) this.GetValue(GameLeaderboardUC.ItemsSourceProperty);
      }
      set
      {
        this.SetValue(GameLeaderboardUC.ItemsSourceProperty, (object) value);
      }
    }

    public GameLeaderboardUC()
    {
      this.InitializeComponent();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameLeaderboardUC gameLeaderboardUc = (GameLeaderboardUC) d;
      List<GameLeaderboardItemHeader> leaderboardItemHeaderList = e.NewValue as List<GameLeaderboardItemHeader>;
      gameLeaderboardUc.itemsControl.ItemsSource = null;
      gameLeaderboardUc.itemsControl.ItemsSource = (IEnumerable) leaderboardItemHeaderList;
    }

    private void LeaderboardItem_OnTap(object sender, GestureEventArgs e)
    {
      GameLeaderboardItemHeader leaderboardItemHeader = ((FrameworkElement) sender).DataContext as GameLeaderboardItemHeader;
      if (leaderboardItemHeader == null || leaderboardItemHeader.UserId <= 0L)
        return;
      Navigator.Current.NavigateToUserProfile(leaderboardItemHeader.UserId, leaderboardItemHeader.UserName, "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GameLeaderboardUC.xaml", UriKind.Relative));
      this.itemsControl = (ItemsControl) this.FindName("itemsControl");
    }
  }
}
