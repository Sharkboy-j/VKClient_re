using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.UC
{
  public class GameNewsHeaderUC : UserControl
  {
    public static readonly DependencyProperty GameGroupIdProperty = DependencyProperty.Register("GameGroupId", typeof (long), typeof (GameNewsHeaderUC), new PropertyMetadata((object) 0L));
    public static readonly DependencyProperty IsSubscribedProperty = DependencyProperty.Register("IsSubscribed", typeof (bool?), typeof (GameNewsHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GameNewsHeaderUC.OnIsSubscribedChanged)));
    private bool _isLoading;
    internal TextBlock textBlockSubscribe;
    private bool _contentLoaded;

    public long GameGroupId
    {
      get
      {
        return (long) this.GetValue(GameNewsHeaderUC.GameGroupIdProperty);
      }
      set
      {
        this.SetValue(GameNewsHeaderUC.GameGroupIdProperty, (object) value);
      }
    }

    public bool? IsSubscribed
    {
      get
      {
        return new bool?((bool) this.GetValue(GameNewsHeaderUC.IsSubscribedProperty));
      }
      set
      {
        this.SetValue(GameNewsHeaderUC.IsSubscribedProperty, (object) value);
      }
    }

    public GameNewsHeaderUC()
    {
      this.InitializeComponent();
    }

    private static void OnIsSubscribedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GameNewsHeaderUC gameNewsHeaderUc = d as GameNewsHeaderUC;
      if (gameNewsHeaderUc == null)
        return;
      bool? nullable = (bool?) e.NewValue;
      if (!nullable.HasValue)
      {
        gameNewsHeaderUc.textBlockSubscribe.Visibility = Visibility.Collapsed;
      }
      else
      {
        gameNewsHeaderUc.textBlockSubscribe.Visibility = Visibility.Visible;
        gameNewsHeaderUc.textBlockSubscribe.Text = nullable.Value ? CommonResources.Games_NewsUnsubscribe : CommonResources.Games_NewsSubscribe;
      }
    }

    private void SubscribeUnsubscribe_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.GameGroupId == 0L || (!this.IsSubscribed.HasValue || this._isLoading))
        return;
      this._isLoading = true;
      GroupsService current = GroupsService.Current;
      if (this.IsSubscribed.Value)
        current.Leave(this.GameGroupId, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.IsSubscribed = new bool?(false);
          this._isLoading = false;
        }))));
      else
        current.Join(this.GameGroupId, false, (Action<BackendResult<OwnCounters, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.IsSubscribed = new bool?(true);
          this._isLoading = false;
        }))));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GameNewsHeaderUC.xaml", UriKind.Relative));
      this.textBlockSubscribe = (TextBlock) this.FindName("textBlockSubscribe");
    }
  }
}
