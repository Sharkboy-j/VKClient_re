using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;

namespace VKClient.Common.UC
{
  public class GameNotificationsSettingsUC : UserControl
  {
    public static readonly DependencyProperty GameIdProperty = DependencyProperty.Register("GameId", typeof (long), typeof (GameNotificationsSettingsUC), new PropertyMetadata((object) 0L));
    private bool _contentLoaded;

    public long GameId
    {
      get
      {
        return (long) this.GetValue(GameNotificationsSettingsUC.GameIdProperty);
      }
      set
      {
        this.SetValue(GameNotificationsSettingsUC.GameIdProperty, (object) value);
      }
    }

    public GameNotificationsSettingsUC()
    {
      this.InitializeComponent();
    }

    private void GameNotificationsSettings_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.GameId < 1L)
        return;
      Navigator.Current.NavigateToGameSettings(this.GameId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GameNotificationsSettingsUC.xaml", UriKind.Relative));
    }
  }
}
