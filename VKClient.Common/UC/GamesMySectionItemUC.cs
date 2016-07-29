using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class GamesMySectionItemUC : UserControl, INotifyPropertyChanged, IHandle<GameRequestReadEvent>, IHandle, IHandle<GameDisconnectedEvent>
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (ObservableCollection<GameHeader>), typeof (GamesMySectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesMySectionItemUC.OnItemsSourceChanged)));
    public static readonly DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof (FrameworkElement), typeof (GamesMySectionItemUC), new PropertyMetadata(null));
    private ObservableCollection<GameHeader> _actualItemsSource;
    internal ListBox listBoxGames;
    private bool _contentLoaded;

    public ObservableCollection<GameHeader> ItemsSource
    {
      get
      {
        return (ObservableCollection<GameHeader>) this.GetValue(GamesMySectionItemUC.ItemsSourceProperty);
      }
      set
      {
        this.SetDPValue(GamesMySectionItemUC.ItemsSourceProperty, (object) value, "ItemsSource");
      }
    }

    public FrameworkElement Root
    {
      get
      {
        return (FrameworkElement) this.GetValue(GamesMySectionItemUC.RootProperty);
      }
      set
      {
        this.SetValue(GamesMySectionItemUC.RootProperty, (object) value);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public GamesMySectionItemUC()
    {
      this.InitializeComponent();
      ((FrameworkElement) this.Content).DataContext = (object) this;
      EventAggregator.Current.Subscribe((object) this);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      GamesMySectionItemUC gamesMySectionItemUc = d as GamesMySectionItemUC;
      if (gamesMySectionItemUc == null)
        return;
      IEnumerable<GameHeader> source = e.NewValue as IEnumerable<GameHeader>;
      gamesMySectionItemUc.listBoxGames.ItemsSource = null;
      gamesMySectionItemUc._actualItemsSource = (ObservableCollection<GameHeader>) null;
      if (source == null)
        return;
      gamesMySectionItemUc._actualItemsSource = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>) source.OrderByDescending<GameHeader, long>((Func<GameHeader, long>) (game => game.LastRequestDate)));
      gamesMySectionItemUc.listBoxGames.ItemsSource = (IEnumerable) gamesMySectionItemUc._actualItemsSource;
    }

    private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
    {
      this.SetValue(property, value);
      if (this.PropertyChanged == null)
        return;
      this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    private void Game_OnTapped(object sender, GestureEventArgs e)
    {
      this.OpenGame(this.listBoxGames.SelectedIndex);
    }

    private void OpenGame(int gameIndex)
    {
      if (gameIndex < 0)
        return;
      FramePageUtils.CurrentPage.OpenGamesPopup(new List<object>((IEnumerable<object>) this._actualItemsSource), GamesClickSource.catalog, "", gameIndex, this.Root);
    }

    private void GroupHeader_OnMoreTapped(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToMyGames();
    }

    public void Handle(GameRequestReadEvent data)
    {
      if (this.ItemsSource == null)
        return;
      using (IEnumerator<GameHeader> enumerator = this.ItemsSource.Where<GameHeader>((Func<GameHeader, bool>) (game =>
      {
        if (game.Game.id == data.GameRequestHeader.Game.id)
          return game.Requests != null;
        return false;
      })).GetEnumerator())
      {
        if (enumerator.MoveNext())
          enumerator.Current.Requests.Clear();
      }
      this._actualItemsSource = new ObservableCollection<GameHeader>((IEnumerable<GameHeader>) this._actualItemsSource.OrderByDescending<GameHeader, long>((Func<GameHeader, long>) (game => game.LastRequestDate)));
      this.listBoxGames.ItemsSource = (IEnumerable) this._actualItemsSource;
    }

    public void Handle(GameDisconnectedEvent data)
    {
      if (this.ItemsSource == null)
        return;
      using (IEnumerator<GameHeader> enumerator = this.ItemsSource.Where<GameHeader>((Func<GameHeader, bool>) (game => game.Game.id == data.GameId)).GetEnumerator())
      {
        if (enumerator.MoveNext())
          this.ItemsSource.Remove(enumerator.Current);
      }
      this.listBoxGames.ItemsSource = null;
      this.listBoxGames.ItemsSource = (IEnumerable) this.ItemsSource;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/GamesMySectionItemUC.xaml", UriKind.Relative));
      this.listBoxGames = (ListBox) this.FindName("listBoxGames");
    }
  }
}
