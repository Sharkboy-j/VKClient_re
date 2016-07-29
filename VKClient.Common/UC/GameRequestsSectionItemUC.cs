using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class GameRequestsSectionItemUC : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameRequestHeader>), typeof(GameRequestsSectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GameRequestsSectionItemUC.OnItemsSourceChanged)));
        public static readonly DependencyProperty MaxDisplayedItemsCountProperty = DependencyProperty.Register("MaxDisplayedItemsCount", typeof(int), typeof(GameRequestsSectionItemUC), new PropertyMetadata((object)2));
        private List<GameRequestHeader> _gameRequests;
        private ObservableCollection<GameRequestHeaderUC> _actualItemsSource;
        internal GroupFooterUC ucFooter;
        private bool _contentLoaded;

        public List<GameRequestHeader> ItemsSource
        {
            get
            {
                return (List<GameRequestHeader>)this.GetValue(GameRequestsSectionItemUC.ItemsSourceProperty);
            }
            set
            {
                this.SetDPValue(GameRequestsSectionItemUC.ItemsSourceProperty, (object)value, "ItemsSource");
            }
        }

        public int MaxDisplayedItemsCount
        {
            get
            {
                return (int)this.GetValue(GameRequestsSectionItemUC.MaxDisplayedItemsCountProperty);
            }
            set
            {
                this.SetDPValue(GameRequestsSectionItemUC.MaxDisplayedItemsCountProperty, (object)value, "MaxDisplayedItemsCount");
            }
        }

        public ObservableCollection<GameRequestHeaderUC> ActualItemsSource
        {
            get
            {
                return this._actualItemsSource;
            }
            set
            {
                this._actualItemsSource = value;
                this.OnPropertyChanged("ActualItemsSource");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameRequestsSectionItemUC()
        {
            this.InitializeComponent();
            ((FrameworkElement)this.Content).DataContext = (object)this;
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameRequestsSectionItemUC requestsSectionItemUc = d as GameRequestsSectionItemUC;
            if (requestsSectionItemUc == null || !(e.NewValue is List<GameRequestHeader>))
                return;
            requestsSectionItemUc.UpdateData();
        }

        public void MarkAllAsRead()
        {
            if (this.ItemsSource == null)
                return;
            List<long> longList = new List<long>();
            foreach (GameRequestHeaderUC gameRequestHeaderUc in this.ActualItemsSource.Where<GameRequestHeaderUC>((Func<GameRequestHeaderUC, bool>)(item =>
            {
                if (item.DataProvider != null)
                    return !item.DataProvider.IsRead;
                return false;
            })))
            {
                longList.Add(gameRequestHeaderUc.DataProvider.GameRequest.id);
                gameRequestHeaderUc.MarkAsRead();
                EventAggregator.Current.Publish((object)new GameRequestReadEvent(gameRequestHeaderUc.DataProvider));
            }
            if (longList.Count <= 0)
                return;
            AppsService.Instance.MarkRequestAsRead((IEnumerable<long>)longList, (Action<BackendResult<OwnCounters, ResultCode>>)(result =>
            {
                if (result.ResultCode != ResultCode.Succeeded)
                    return;
                CountersManager.Current.Counters = result.ResultData;
            }));
        }

        private void UpdateData()
        {
            this.ActualItemsSource = null;
            if (this.ItemsSource == null)
                return;
            this._gameRequests = this.ItemsSource.Where<GameRequestHeader>((Func<GameRequestHeader, bool>)(item =>
            {
                if (item != null)
                    return !item.IsInvite;
                return false;
            })).ToList<GameRequestHeader>();
            List<GameRequestHeader> gameRequestHeaderList = new List<GameRequestHeader>(this._gameRequests.Where<GameRequestHeader>((Func<GameRequestHeader, bool>)(item => !item.IsRead)));
            if (gameRequestHeaderList.Count < this.MaxDisplayedItemsCount && gameRequestHeaderList.Count < this._gameRequests.Count)
            {
                int count = Math.Min(this.MaxDisplayedItemsCount, this._gameRequests.Count) - gameRequestHeaderList.Count;
                gameRequestHeaderList.AddRange(this._gameRequests.Skip<GameRequestHeader>(gameRequestHeaderList.Count).Take<GameRequestHeader>(count));
            }
            this.ActualItemsSource = new ObservableCollection<GameRequestHeaderUC>();
            foreach (GameRequestHeader gameRequestHeader in gameRequestHeaderList)
            {
                ObservableCollection<GameRequestHeaderUC> actualItemsSource = this.ActualItemsSource;
                GameRequestHeaderUC gameRequestHeaderUc = new GameRequestHeaderUC();
                gameRequestHeaderUc.DataProvider = gameRequestHeader;
                Action<GameRequestHeader, Action> action = new Action<GameRequestHeader, Action>(this.DeleteRequestAction);
                gameRequestHeaderUc.DeleteRequestAction = action;
                actualItemsSource.Add(gameRequestHeaderUc);
            }
            int number = this._gameRequests.Count - this.ActualItemsSource.Count;
            if (number == 1)
            {
                this.ActualItemsSource.Add(new GameRequestHeaderUC()
                {
                    DataProvider = this._gameRequests.Last<GameRequestHeader>(),
                    DeleteRequestAction = new Action<GameRequestHeader, Action>(this.DeleteRequestAction)
                });
                --number;
            }
            this.UpdateFooterVisibility();
            this.ucFooter.FooterText = UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.Games_ShowMoreRequestsOneFrm, CommonResources.Games_ShowMoreRequestsTwoFourFrm, CommonResources.Games_ShowMoreRequestsFiveFrm, true, null, false);
            if (this.ucFooter.Visibility != Visibility.Collapsed || this.ActualItemsSource.Count <= 0)
                return;
            this.ActualItemsSource.Last<GameRequestHeaderUC>().IsSeparatorVisible = false;
        }

        private void DeleteRequestAction(GameRequestHeader gameRequestHeader, Action callback)
        {
            GameRequestHeaderUC uc = this.ActualItemsSource.FirstOrDefault<GameRequestHeaderUC>((Func<GameRequestHeaderUC, bool>)(item => item.DataProvider == gameRequestHeader));
            if (uc == null || uc.DataProvider == null || uc.DataProvider.GameRequest == null)
                return;
            AppsService.Instance.DeleteRequest(uc.DataProvider.GameRequest.id, (Action<BackendResult<OwnCounters, ResultCode>>)(result => Execute.ExecuteOnUIThread((Action)(() =>
            {
                if (result.ResultCode == ResultCode.Succeeded)
                    CountersManager.Current.Counters = result.ResultData;
                this.ItemsSource.Remove(this.ItemsSource.FirstOrDefault<GameRequestHeader>(/*func ?? (func = (*/new Func<GameRequestHeader, bool>(header => header.GameRequest.id == uc.DataProvider.GameRequest.id)));//omg_re
                this.UpdateData();
                if (callback == null)
                    return;
                callback();
            }))));
        }

        private void UpdateFooterVisibility()
        {
            this.ucFooter.Visibility = this.ActualItemsSource.Count < this._gameRequests.Count ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Footer_OnMoreTapped(object sender, EventArgs e)
        {
            for (int count = this.ActualItemsSource.Count; count < this._gameRequests.Count; ++count)
                this.ActualItemsSource.Add(new GameRequestHeaderUC()
                {
                    DataProvider = this._gameRequests[count],
                    DeleteRequestAction = new Action<GameRequestHeader, Action>(this.DeleteRequestAction)
                });
            this.UpdateFooterVisibility();
            if (this.ucFooter.Visibility != Visibility.Collapsed || this.ActualItemsSource.Count <= 0)
                return;
            this.ActualItemsSource.Last<GameRequestHeaderUC>().IsSeparatorVisible = false;
        }

        private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            this.SetValue(property, value);
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/GameRequestsSectionItemUC.xaml", UriKind.Relative));
            this.ucFooter = (GroupFooterUC)this.FindName("ucFooter");
        }
    }
}
