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
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Games;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class GamesFriendsActivityShortHeaderUC : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty DataProviderProperty = DependencyProperty.Register("DataProvider", typeof(GameActivityHeader), typeof(GamesFriendsActivityShortHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityShortHeaderUC.OnDataProviderChanged)));
        public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(GamesFriendsActivityShortHeaderUC), new PropertyMetadata(new PropertyChangedCallback(GamesFriendsActivityShortHeaderUC.OnIsSeparatorVisibleChanged)));
        internal Image imageUserIcon;
        internal TextBlock textBlockDescription;
        internal TextBlock textBlockDate;
        internal Rectangle BottomSeparator;
        private bool _contentLoaded;

        public GameActivityHeader DataProvider
        {
            get
            {
                return (GameActivityHeader)this.GetValue(GamesFriendsActivityShortHeaderUC.DataProviderProperty);
            }
            set
            {
                this.SetDPValue(GamesFriendsActivityShortHeaderUC.DataProviderProperty, (object)value, "DataProvider");
            }
        }

        public bool IsSeparatorVisible
        {
            get
            {
                return (bool)this.GetValue(GamesFriendsActivityShortHeaderUC.IsSeparatorVisibleProperty);
            }
            set
            {
                this.SetDPValue(GamesFriendsActivityShortHeaderUC.IsSeparatorVisibleProperty, (object)value, "IsSeparatorVisible");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GamesFriendsActivityShortHeaderUC()
        {
            this.InitializeComponent();
            ((FrameworkElement)this.Content).DataContext = (object)this;
        }

        private static void OnDataProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GamesFriendsActivityShortHeaderUC activityShortHeaderUc = d as GamesFriendsActivityShortHeaderUC;
            if (activityShortHeaderUc == null)
                return;
            GameActivityHeader gameActivityHeader = e.NewValue as GameActivityHeader;
            if (gameActivityHeader == null)
                return;
            activityShortHeaderUc.imageUserIcon.Tag = (object)gameActivityHeader.User;
            ImageLoader.SetUriSource(activityShortHeaderUc.imageUserIcon, gameActivityHeader.User.photo_max);
            activityShortHeaderUc.textBlockDate.Text = UIStringFormatterHelper.FormatDateTimeForUI(gameActivityHeader.GameActivity.date);
            activityShortHeaderUc.textBlockDescription.Tag = (object)gameActivityHeader.Game;
            activityShortHeaderUc.textBlockDescription.Inlines.Clear();
            List<Inline> list = gameActivityHeader.ComposeActivityText(false);
            if (list.IsNullOrEmpty())
                return;
            for (int index = 0; index < list.Count; ++index)
            {
                Run run = list[index] as Run;
                if (run != null)
                {
                    activityShortHeaderUc.textBlockDescription.Inlines.Add((Inline)run);
                    if (index < list.Count - 1)
                        run.Text += " ";
                }
            }
        }

        private static void OnIsSeparatorVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GamesFriendsActivityShortHeaderUC activityShortHeaderUc = d as GamesFriendsActivityShortHeaderUC;
            if (activityShortHeaderUc == null)
                return;
            bool flag = (bool)e.NewValue;
            activityShortHeaderUc.BottomSeparator.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetDPValue(DependencyProperty property, object value, [CallerMemberName] string propertyName = null)
        {
            this.SetValue(property, value);
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        private void User_OnTap(object sender, GestureEventArgs e)
        {
            User user = this.imageUserIcon.Tag as User;
            if (user == null)
                return;
            Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
        }

        private void Description_OnTap(object sender, GestureEventArgs e)
        {
            User user = this.imageUserIcon.Tag as User;
            if (user == null)
                return;
            Navigator.Current.NavigateToUserProfile(user.uid, user.Name, "", false);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/GamesFriendsActivityShortHeaderUC.xaml", UriKind.Relative));
            this.imageUserIcon = (Image)this.FindName("imageUserIcon");
            this.textBlockDescription = (TextBlock)this.FindName("textBlockDescription");
            this.textBlockDate = (TextBlock)this.FindName("textBlockDate");
            this.BottomSeparator = (Rectangle)this.FindName("BottomSeparator");
        }
    }
}
