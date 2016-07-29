using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesFriendsActivityHeaderUC : UserControl, INotifyPropertyChanged
  {
    public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof (GameActivityHeader), typeof (GamesFriendsActivityHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityHeaderUC.OnDataProviderChanged)));
    public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof (bool), typeof (GamesFriendsActivityHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityHeaderUC.OnIsSeparatorVisibleChanged)));
    internal Image imageUser;
    internal TextBlock textBlockDescription;
    internal Image imageGame;
    internal TextBlock textBlockDate;
    internal Rectangle rectSeparator;
    private bool _contentLoaded;

    public GameActivityHeader DataProvider
    {
      get
      {
        return (GameActivityHeader) this.GetValue(GamesFriendsActivityHeaderUC.DataProviderProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityHeaderUC.DataProviderProperty, (object) value, "DataProvider");
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) this.GetValue(GamesFriendsActivityHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GamesFriendsActivityHeaderUC.IsSeparatorVisibleProperty, (object) value, "IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesFriendsActivityHeaderUC()
    {
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = (object) this;
    }

    private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityHeaderUC activityHeaderUc = d as GamesFriendsActivityHeaderUC;
      if (activityHeaderUc == null)
        return;
      GameActivityHeader gameActivityHeader = e.NewValue as GameActivityHeader;
      if (gameActivityHeader == null)
        return;
      ImageLoader.SetUriSource(activityHeaderUc.imageUser, gameActivityHeader.User.photo_max);
      ImageLoader.SetUriSource(activityHeaderUc.imageGame, gameActivityHeader.Game.icon_100);
      activityHeaderUc.textBlockDescription.Inlines.Clear();
      List<Inline> list = gameActivityHeader.ComposeActivityText(true);
      if (!list.IsNullOrEmpty())
      {
        for (int index = 0; index < list.Count; ++index)
        {
          Run run = list[index] as Run;
          if (run != null)
          {
            activityHeaderUc.textBlockDescription.Inlines.Add((Inline) run);
            if (index < list.Count - 1)
              run.Text += " ";
          }
        }
      }
      activityHeaderUc.textBlockDate.Text = UIStringFormatterHelper.FormatDateTimeForUI(gameActivityHeader.GameActivity.date);
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesFriendsActivityHeaderUC activityHeaderUc = d as GamesFriendsActivityHeaderUC;
      if (activityHeaderUc == null)
        return;
      bool flag = (bool) e.NewValue;
      activityHeaderUc.rectSeparator.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      this.SetValue(property, value);
      PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
      if (changedEventHandler == null)
        return;
      PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
      changedEventHandler((object) this, e);
    }

    private void User_OnTap(object sender, GestureEventArgs e)
    {
      User user = this.DataProvider.User;
      if (user == null)
        return;
      Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
    }

    private void Game_OnTap(object sender, GestureEventArgs e)
    {
      GamesFriendsActivityHeaderUC.OpenGame(this.DataProvider.Game);
    }

    private void Description_OnTap(object sender, GestureEventArgs e)
    {
      GamesFriendsActivityHeaderUC.OpenGame(this.DataProvider.Game);
    }

    private static void OpenGame(Game game)
    {
      if (game == null)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>()
      {
        (object) new GameHeader(game)
      }, GamesClickSource.activity, "", 0, null);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GamesFriendsActivityHeaderUC.xaml", UriKind.Relative));
      this.imageUser = (Image) this.FindName("imageUser");
      this.textBlockDescription = (TextBlock) this.FindName("textBlockDescription");
      this.imageGame = (Image) this.FindName("imageGame");
      this.textBlockDate = (TextBlock) this.FindName("textBlockDate");
      this.rectSeparator = (Rectangle) this.FindName("rectSeparator");
    }
  }
}
