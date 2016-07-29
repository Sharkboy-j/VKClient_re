using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class GamesInvitesSectionItemUC : UserControl, INotifyPropertyChanged, IHandle<GameInvitationHiddenEvent>, IHandle
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<GameRequestHeader>), typeof(GamesInvitesSectionItemUC), new PropertyMetadata(new PropertyChangedCallback(GamesInvitesSectionItemUC.OnItemsSourceChanged)));
        public const int MAX_DISPLAYED_ITEMS_COUNT = 2;
        internal GroupHeaderUC HeaderUC;
        internal ItemsControl InvitesListBox;
        internal GroupFooterUC FooterUC;
        private bool _contentLoaded;

        public List<GameRequestHeader> ItemsSource
        {
            get
            {
                return (List<GameRequestHeader>)this.GetValue(GamesInvitesSectionItemUC.ItemsSourceProperty);
            }
            set
            {
                this.SetDPValue(GamesInvitesSectionItemUC.ItemsSourceProperty, (object)value, "ItemsSource");
            }
        }

        public event EventHandler ItemsCleared;

        public event PropertyChangedEventHandler PropertyChanged;

        public GamesInvitesSectionItemUC()
        {
            this.InitializeComponent();
            ((FrameworkElement)this.Content).DataContext = (object)this;
            EventAggregator.Current.Subscribe((object)this);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GamesInvitesSectionItemUC invitesSectionItemUc = d as GamesInvitesSectionItemUC;
            if (invitesSectionItemUc == null || !(e.NewValue is List<GameRequestHeader>))
                return;
            invitesSectionItemUc.UpdateData();
        }

        private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            this.SetValue(property, value);
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateData()
        {
            this.UpdateHeaderTitle();
            this.UpdateFooterVisibility();
            this.RebindItems();
            int count = this.ItemsSource.Count;
            if (count > 2 || count <= 0)
                return;
            if (count == 1)
                this.ItemsSource[0].IsSeparatorVisible = false;
            else
                this.ItemsSource[count - 1].IsSeparatorVisible = false;
        }

        private void UpdateHeaderTitle()
        {
            this.HeaderUC.Title = UIStringFormatterHelper.FormatNumberOfSomething(this.ItemsSource.Count, CommonResources.OneInviteTitleFrm, CommonResources.TwoFourInvitesTitleFrm, CommonResources.FiveInvitesTitleFrm, true, null, false).ToLowerInvariant();
        }

        public void UpdateFooterVisibility()
        {
            this.FooterUC.Visibility = this.ItemsSource.Count > 2 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RebindItems()
        {
            this.InvitesListBox.ItemsSource = null;
            this.InvitesListBox.ItemsSource = (IEnumerable)this.ItemsSource.Take<GameRequestHeader>(2).ToList<GameRequestHeader>();
        }

        private void Footer_OnMoreTapped(object sender, EventArgs e)
        {
            Navigator.Current.NavigateToGamesInvites();
        }

        public void Handle(GameInvitationHiddenEvent message)
        {
            this.RemoveInvitation(message.Invitation);
        }

        private void RemoveInvitation(GameRequestHeader invitation)
        {
            if (this.ItemsSource == null)
                return;
            long invitationId = invitation.GameRequest.id;
            using (IEnumerator<GameRequestHeader> enumerator = this.ItemsSource.Where<GameRequestHeader>((Func<GameRequestHeader, bool>)(item => item.GameRequest.id == invitationId)).GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return;
                this.ItemsSource.Remove(enumerator.Current);
                if (this.ItemsSource.Count > 0)
                {
                    this.UpdateData();
                }
                else
                {
                    if (this.ItemsCleared == null)
                        return;
                    this.ItemsCleared((object)this, EventArgs.Empty);
                }
            }
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/GamesInvitesSectionItemUC.xaml", UriKind.Relative));
            this.HeaderUC = (GroupHeaderUC)this.FindName("HeaderUC");
            this.InvitesListBox = (ItemsControl)this.FindName("InvitesListBox");
            this.FooterUC = (GroupFooterUC)this.FindName("FooterUC");
        }
    }
}
