using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GameViewHeaderUC : UserControl
  {
    public static readonly DependencyProperty MoreActionsVisibilityProperty = DependencyProperty.Register("MoreActionsVisibility", typeof (Visibility), typeof (GameViewHeaderUC), new PropertyMetadata((object) Visibility.Visible, new PropertyChangedCallback(GameViewHeaderUC.OnMoreActionsVisibilityChanged)));
    public static readonly DependencyProperty GameHeaderProperty = DependencyProperty.Register("GameHeader", typeof (GameHeader), typeof (GameViewHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameViewHeaderUC.OnGameHeaderChanged)));
    public static readonly DependencyProperty NextGameHeaderProperty = DependencyProperty.Register("NextGameHeader", typeof (GameHeader), typeof (GameViewHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameViewHeaderUC.OnNextGameHeaderChanged)));
    public static readonly DependencyProperty IsSystemTrayPlaceholderVisibleProperty = DependencyProperty.Register("IsSystemTrayPlaceholderVisible", typeof (bool), typeof (GameViewHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameViewHeaderUC.OnIsSystemTrayPlaceholderVisibleChanged)));
    internal Border borderSystemTrayPlaceholder;
    internal Grid HeaderPanel;
    internal Image imageIcon;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockGenre;
    internal Border ucMoreActions;
    internal Border borderNextGame;
    internal Image imageNextIcon;
    private bool _contentLoaded;

    public Visibility MoreActionsVisibility
    {
      get
      {
        return (Visibility) this.GetValue(GameViewHeaderUC.MoreActionsVisibilityProperty);
      }
      set
      {
        this.SetValue(GameViewHeaderUC.MoreActionsVisibilityProperty, (object) value);
      }
    }

    public GameHeader GameHeader
    {
      get
      {
        return (GameHeader) this.GetValue(GameViewHeaderUC.GameHeaderProperty);
      }
      set
      {
        this.SetValue(GameViewHeaderUC.GameHeaderProperty, (object) value);
      }
    }

    public GameHeader NextGameHeader
    {
      get
      {
        return (GameHeader) this.GetValue(GameViewHeaderUC.NextGameHeaderProperty);
      }
      set
      {
        this.SetValue(GameViewHeaderUC.NextGameHeaderProperty, (object) value);
      }
    }

    public bool IsSystemTrayPlaceholderVisible
    {
      get
      {
        return (bool) this.GetValue(GameViewHeaderUC.IsSystemTrayPlaceholderVisibleProperty);
      }
      set
      {
        this.SetValue(GameViewHeaderUC.IsSystemTrayPlaceholderVisibleProperty, (object) value);
      }
    }

    public GameViewHeaderUC()
    {
      this.InitializeComponent();
    }

    private static void OnMoreActionsVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameViewHeaderUC gameViewHeaderUc = d as GameViewHeaderUC;
      Visibility result;
      if (gameViewHeaderUc == null || e.NewValue == null || !Enum.TryParse<Visibility>(e.NewValue.ToString(), out result))
        return;
      gameViewHeaderUc.ucMoreActions.Visibility = result;
    }

    private static void OnGameHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameViewHeaderUC gameViewHeaderUc = d as GameViewHeaderUC;
      if (gameViewHeaderUc == null)
        return;
      if (!(e.NewValue is GameHeader))
        gameViewHeaderUc.ResetData();
      else
        gameViewHeaderUc.UpdateData();
    }

    private void ResetData()
    {
      ImageLoader.SetUriSource(this.imageIcon, "");
      this.textBlockGenre.Text = "";
      this.textBlockTitle.Text = "";
      this.ucMoreActions.Visibility = Visibility.Collapsed;
    }

    private void UpdateData()
    {
      ImageLoader.SetUriSource(this.imageIcon, this.GameHeader.Icon);
      this.textBlockGenre.Text = this.GameHeader.Genre;
      this.textBlockTitle.Text = this.GameHeader.Title;
      GameViewHeaderUC.CorrectTextName(this.textBlockTitle);
      this.ucMoreActions.Visibility = this.GameHeader.IsInstalled ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void CorrectTextName(TextBlock textName)
    {
      if (string.IsNullOrWhiteSpace(textName.Text) || textName.ActualWidth <= 300.0)
        return;
      int int32 = Convert.ToInt32(300.0 / (textName.ActualWidth / (double) textName.Text.Length));
      if (int32 >= textName.Text.Length)
        return;
      textName.Text = textName.Text.Substring(0, int32).Trim() + "...";
      for (int index = 0; textName.ActualWidth > 300.0 && index < 10; ++index)
        textName.Text = textName.Text.Substring(0, textName.Text.Length - 4).Trim() + "...";
    }

    private static void OnNextGameHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameViewHeaderUC gameViewHeaderUc = d as GameViewHeaderUC;
      if (gameViewHeaderUc == null)
        return;
      if (!(e.NewValue is GameHeader))
        gameViewHeaderUc.ResetNextData();
      else
        gameViewHeaderUc.UpdateNextData();
    }

    private void ResetNextData()
    {
      this.borderNextGame.Visibility = Visibility.Collapsed;
      ImageLoader.SetUriSource(this.imageNextIcon, "");
    }

    private void UpdateNextData()
    {
      this.borderNextGame.Visibility = Visibility.Visible;
      ImageLoader.SetUriSource(this.imageNextIcon, this.NextGameHeader.Icon);
    }

    private static void OnIsSystemTrayPlaceholderVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameViewHeaderUC gameViewHeaderUc = d as GameViewHeaderUC;
      if (gameViewHeaderUc == null || !(e.NewValue is bool))
        return;
      gameViewHeaderUc.borderSystemTrayPlaceholder.Visibility = (bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void MoreActions_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      List<MenuItem> menuItems = new List<MenuItem>();
      MenuItem menuItem1 = new MenuItem();
      string notificationsSettings = CommonResources.Games_NotificationsSettings;
      menuItem1.Header = (object) notificationsSettings;
      MenuItem menuItem2 = menuItem1;
      menuItem2.Click += new RoutedEventHandler(this.MenuItemNotificationsSettings_OnClick);
      MenuItem menuItem3 = menuItem2;
      menuItems.Add(menuItem3);
      Grid grid = this.HeaderPanel;
      GameViewHeaderUC.OpenMenu(menuItems, (DependencyObject) grid);
    }

    private void MenuItemNotificationsSettings_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (this.GameHeader == null || this.GameHeader.Game == null)
        return;
      Navigator.Current.NavigateToGameSettings(this.GameHeader.Game.id);
    }

    private static void OpenMenu(List<MenuItem> menuItems, DependencyObject sender)
    {
      if (menuItems.IsNullOrEmpty())
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      contextMenu1.Background = (Brush) solidColorBrush1;
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      contextMenu1.Foreground = (Brush) solidColorBrush2;
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in menuItems)
        contextMenu2.Items.Add((object) menuItem);
      ContextMenuService.SetContextMenu(sender, contextMenu2);
      contextMenu2.IsOpen = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GameViewHeaderUC.xaml", UriKind.Relative));
      this.borderSystemTrayPlaceholder = (Border) this.FindName("borderSystemTrayPlaceholder");
      this.HeaderPanel = (Grid) this.FindName("HeaderPanel");
      this.imageIcon = (Image) this.FindName("imageIcon");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockGenre = (TextBlock) this.FindName("textBlockGenre");
      this.ucMoreActions = (Border) this.FindName("ucMoreActions");
      this.borderNextGame = (Border) this.FindName("borderNextGame");
      this.imageNextIcon = (Image) this.FindName("imageNextIcon");
    }
  }
}
