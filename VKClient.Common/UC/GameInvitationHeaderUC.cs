using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GameInvitationHeaderUC : UserControl
  {
    public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof (GameRequestHeader), typeof (GameInvitationHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameInvitationHeaderUC.OnDataProviderChanged)));
    public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof (bool), typeof (GameInvitationHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameInvitationHeaderUC.OnIsSeparatorVisibleChanged)));
    private bool _isInPlayHandler;
    internal Image UserIconImage;
    internal TextBlock InvitationTextBlock;
    internal Image GameIconImage;
    internal TextBlock GameTitleTextBlock;
    internal TextBlock GameGenreTextBlock;
    internal Rectangle BottomSeparator;
    private bool _contentLoaded;

    public GameRequestHeader DataProvider
    {
      get
      {
        return (GameRequestHeader) this.GetValue(GameInvitationHeaderUC.DataProviderProperty);
      }
      set
      {
        this.SetDPValue(GameInvitationHeaderUC.DataProviderProperty, (object) value, "DataProvider");
      }
    }

    public bool IsSeparatorVisible
    {
      get
      {
        return (bool) this.GetValue(GameInvitationHeaderUC.IsSeparatorVisibleProperty);
      }
      set
      {
        this.SetDPValue(GameInvitationHeaderUC.IsSeparatorVisibleProperty, (object) value, "IsSeparatorVisible");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GameInvitationHeaderUC()
    {
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = (object) this;
    }

    private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameInvitationHeaderUC invitationHeaderUc = d as GameInvitationHeaderUC;
      if (invitationHeaderUc == null)
        return;
      GameRequestHeader gameRequestHeader = e.NewValue as GameRequestHeader;
      if (gameRequestHeader == null)
        return;
      Game game = gameRequestHeader.Game;
      User user = gameRequestHeader.User;
      invitationHeaderUc.UserIconImage.Tag = (object) user;
      ImageLoader.SetUriSource(invitationHeaderUc.UserIconImage, user.photo_max);
      invitationHeaderUc.GameIconImage.Tag = (object) game;
      ImageLoader.SetUriSource(invitationHeaderUc.GameIconImage, game.icon_100);
      invitationHeaderUc.GameTitleTextBlock.Text = game.title;
      invitationHeaderUc.GameGenreTextBlock.Text = game.genre;
      List<Inline> list = GameInvitationHeaderUC.ComposeInvitationText(user.Name);
      if (list.IsNullOrEmpty())
        return;
      invitationHeaderUc.InvitationTextBlock.Inlines.Clear();
      foreach (Inline inline in list)
        invitationHeaderUc.InvitationTextBlock.Inlines.Add(inline);
    }

    private static List<Inline> ComposeInvitationText(string userName)
    {
      FontFamily fontFamily1 = new FontFamily("Segoe WP Semilight");
      Brush brush1 = (Brush) Application.Current.Resources["PhoneVKSubtleBrush"];
      List<Inline> inlineList = new List<Inline>();
      inlineList.Add((Inline) new Run() { Text = userName });
      Run run = new Run();
      run.Text = " " + CommonResources.Games_FriendInvitedToGame;
      FontFamily fontFamily2 = fontFamily1;
      run.FontFamily = fontFamily2;
      Brush brush2 = brush1;
      run.Foreground = brush2;
      inlineList.Add((Inline) run);
      return inlineList;
    }

    private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameInvitationHeaderUC invitationHeaderUc = d as GameInvitationHeaderUC;
      if (invitationHeaderUc == null)
        return;
      bool flag = (bool) e.NewValue;
      invitationHeaderUc.BottomSeparator.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      this.SetValue(property, value);
      if (this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    private void User_OnTap(object sender, GestureEventArgs e)
    {
      User user = this.UserIconImage.Tag as User;
      if (user == null)
        return;
      Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
    }

    private void Game_OnTap(object sender, GestureEventArgs e)
    {
      Game game = this.GameIconImage.Tag as Game;
      if (game == null)
        return;
      GameRequest gameRequest = this.DataProvider.GameRequest;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>()
      {
        (object) new GameHeader(game)
      }, GamesClickSource.request, gameRequest.name, 0, null);
    }

    private async void PlayButton_OnClicked(object sender, RoutedEventArgs e)
    {
      if (this._isInPlayHandler)
        return;
      this._isInPlayHandler = true;
      GameInvitationHeaderUC.HideInvitation(this.DataProvider);
      Game game = this.DataProvider.Game;
      GameRequest gameRequest = this.DataProvider.GameRequest;
      bool flag = InstalledPackagesFinder.Instance.IsPackageInstalled(game.platform_id);
      EventAggregator.Current.Publish((object) new GamesActionEvent()
      {
        game_id = game.id,
        visit_source = AppGlobalStateManager.Current.GlobalState.GamesVisitSource,
        action_type = (GamesActionType) (flag ? 0 : 1),
        click_source = GamesClickSource.request,
        request_name = gameRequest.name
      });
      await Task.Delay(1000);
      Navigator.Current.OpenGame(game);
      this._isInPlayHandler = false;
    }

    private void HideButton_OnClicked(object sender, RoutedEventArgs e)
    {
      GameInvitationHeaderUC.HideInvitation(this.DataProvider);
    }

    private static void HideInvitation(GameRequestHeader gameRequest)
    {
      AppsService.Instance.DeleteRequest(gameRequest.GameRequest.id, (Action<BackendResult<OwnCounters, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        CountersManager.Current.Counters = result.ResultData;
        EventAggregator.Current.Publish((object) new GameInvitationHiddenEvent(gameRequest));
      }));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GameInvitationHeaderUC.xaml", UriKind.Relative));
      this.UserIconImage = (Image) this.FindName("UserIconImage");
      this.InvitationTextBlock = (TextBlock) this.FindName("InvitationTextBlock");
      this.GameIconImage = (Image) this.FindName("GameIconImage");
      this.GameTitleTextBlock = (TextBlock) this.FindName("GameTitleTextBlock");
      this.GameGenreTextBlock = (TextBlock) this.FindName("GameGenreTextBlock");
      this.BottomSeparator = (Rectangle) this.FindName("BottomSeparator");
    }
  }
}
