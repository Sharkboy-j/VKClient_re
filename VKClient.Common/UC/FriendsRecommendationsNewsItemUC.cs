using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class FriendsRecommendationsNewsItemUC : UserControlVirtualizable
  {
    internal Grid MainPanel;
    private bool _contentLoaded;

    public FriendsRecommendationsNewsItemUC(NewsItemDataWithUsersAndGroupsInfo newsItem)
    {
      this.InitializeComponent();
      this.MainPanel.Children.Add((UIElement) new FriendsRecommendationsUC(newsItem));
    }

    private void ShowAllButton_OnTapped(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToFriendsSuggestions();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FriendsRecommendationsNewsItemUC.xaml", UriKind.Relative));
      this.MainPanel = (Grid) this.FindName("MainPanel");
    }
  }
}
