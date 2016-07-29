using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class FriendRequestUC : UserControl
  {
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof (FriendRequest), typeof (FriendRequestUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestUC.OnModelChanged)));
    public static readonly DependencyProperty ProfilesProperty = DependencyProperty.Register("Profiles", typeof (User[]), typeof (FriendRequestUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestUC.OnProfilesChanged)));
    public static readonly DependencyProperty IsSuggestedFriendProperty = DependencyProperty.Register("IsSuggestedFriend", typeof (bool?), typeof (FriendRequestUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestUC.OnIsSuggestedFriendChanged)));
    public static readonly DependencyProperty NeedBottomSeparatorLineProperty = DependencyProperty.Register("NeedBottomSeparatorLine", typeof (bool), typeof (FriendRequestUC), new PropertyMetadata(new PropertyChangedCallback(FriendRequestUC.OnNeedBottomSeparatorLineChanged)));
    internal Image RequestPhoto;
    internal TextBlock RequestName;
    internal TextBlock RequestOccupation;
    internal TextBlock RequestMessage;
    internal Grid RecommenderPanel;
    internal TextBlock RecommenderName;
    internal Grid MutualFriendsPanel;
    internal TextBlock MutualFriendsCountBlock;
    internal StackPanel MutualFriendsPhotosPanel;
    internal Button AddButton;
    internal Button HideButton;
    internal Rectangle BottomSeparatorRectangle;
    private bool _contentLoaded;

    public FriendRequest Model
    {
      get
      {
        return (FriendRequest) this.GetValue(FriendRequestUC.ModelProperty);
      }
      set
      {
        this.SetValue(FriendRequestUC.ModelProperty, (object) value);
      }
    }

    public User[] Profiles
    {
      get
      {
        return (User[]) this.GetValue(FriendRequestUC.ProfilesProperty);
      }
      set
      {
        this.SetValue(FriendRequestUC.ProfilesProperty, (object) value);
      }
    }

    public bool? IsSuggestedFriend
    {
      get
      {
        return (bool?) this.GetValue(FriendRequestUC.IsSuggestedFriendProperty);
      }
      set
      {
        this.SetValue(FriendRequestUC.IsSuggestedFriendProperty, (object) value);
      }
    }

    public bool NeedBottomSeparatorLine
    {
      get
      {
        return (bool) this.GetValue(FriendRequestUC.NeedBottomSeparatorLineProperty);
      }
      set
      {
        this.SetValue(FriendRequestUC.NeedBottomSeparatorLineProperty, (object) value);
      }
    }

    public FriendRequestUC()
    {
      this.InitializeComponent();
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((FriendRequestUC) d).UpdateDataView();
    }

    private static void OnProfilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((FriendRequestUC) d).UpdateDataView();
    }

    private static void OnIsSuggestedFriendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((FriendRequestUC) d).UpdateDataView();
    }

    private static void OnNeedBottomSeparatorLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      ((FriendRequestUC) d).BottomSeparatorRectangle.Visibility = (bool) e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateDataView()
    {
      if (this.Model == null || this.Profiles == null || !this.IsSuggestedFriend.HasValue)
        return;
      this.RequestOccupation.Visibility = Visibility.Collapsed;
      this.RequestMessage.Visibility = Visibility.Collapsed;
      this.RecommenderPanel.Visibility = Visibility.Collapsed;
      this.MutualFriendsPanel.Visibility = Visibility.Collapsed;
      this.MutualFriendsPanel.Children.Clear();
      User user1 = ((IEnumerable<User>) this.Profiles).FirstOrDefault<User>((Func<User, bool>) (profile => profile.id == this.Model.user_id));
      if (user1 == null)
        return;
      User user2 = (User) null;
      bool? isSuggestedFriend = this.IsSuggestedFriend;
      if (isSuggestedFriend.Value)
      {
        user2 = ((IEnumerable<User>) this.Profiles).First<User>((Func<User, bool>) (profile => profile.id == this.Model.from));
        if (user2 == null)
          return;
      }
      this.RequestName.Text = user1.Name;
      ImageLoader.SetUriSource(this.RequestPhoto, user1.photo_max);
      if (user1.occupation != null && string.IsNullOrWhiteSpace(user1.occupation.name))
      {
        this.RequestOccupation.Text = user1.occupation.name;
        this.RequestOccupation.Visibility = Visibility.Visible;
      }
      if (!string.IsNullOrWhiteSpace(this.Model.message))
      {
        this.RequestMessage.Text = Extensions.ForUI(this.Model.message);
        this.RequestMessage.Visibility = Visibility.Visible;
      }
      isSuggestedFriend = this.IsSuggestedFriend;
      if (isSuggestedFriend.Value)
      {
        this.RecommenderName.Text = user2.NameGen;
        this.RecommenderPanel.Visibility = Visibility.Visible;
      }
      if (this.Model.mutual == null)
        return;
      this.MutualFriendsCountBlock.Text = UIStringFormatterHelper.FormatNumberOfSomething(this.Model.mutual.count, CommonResources.OneCommonFriendFrm, CommonResources.TwoFourCommonFriendsFrm, CommonResources.FiveCommonFriendsFrm, true, null, false);
      this.MutualFriendsPanel.Visibility = Visibility.Visible;
      foreach (long user3 in this.Model.mutual.users)
      {
        foreach (User profile in this.Profiles)
        {
          if (user3 == profile.id)
          {
            Ellipse ellipse = new Ellipse();
            Style style = (Style) Application.Current.Resources["PhotoPlaceholderEllipse"];
            ellipse.Style = style;
            int num1 = 0;
            ellipse.HorizontalAlignment = (HorizontalAlignment) num1;
            int num2 = 0;
            ellipse.VerticalAlignment = (VerticalAlignment) num2;
            double num3 = 40.0;
            ellipse.Height = num3;
            double num4 = 40.0;
            ellipse.Width = num4;
            this.MutualFriendsPhotosPanel.Children.Add((UIElement) ellipse);
            Image image1 = new Image();
            EllipseGeometry ellipseGeometry = new EllipseGeometry();
            ellipseGeometry.RadiusX = 20.0;
            ellipseGeometry.RadiusY = 20.0;
            Point point = new Point(20.0, 20.0);
            ellipseGeometry.Center = point;
            image1.Clip = (Geometry) ellipseGeometry;
            int num5 = 0;
            image1.HorizontalAlignment = (HorizontalAlignment) num5;
            int num6 = 0;
            image1.VerticalAlignment = (VerticalAlignment) num6;
            Thickness thickness = new Thickness(-40.0, 0.0, 4.0, 0.0);
            image1.Margin = thickness;
            int num7 = 2;
            image1.Stretch = (Stretch) num7;
            double num8 = 40.0;
            image1.Height = num8;
            double num9 = 40.0;
            image1.Width = num9;
            Image image2 = image1;
            ImageLoader.SetUriSource(image2, profile.photo_max);
            this.MutualFriendsPhotosPanel.Children.Add((UIElement) image2);
            break;
          }
        }
        if (this.MutualFriendsPhotosPanel.Children.Count == 8)
          break;
      }
    }

    private void Request_OnTapped(object sender, GestureEventArgs e)
    {
      Navigator.Current.NavigateToUserProfile(this.Model.user_id, this.RequestName.Text, "", false);
    }

    private void RecommenderName_OnTapped(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      Navigator.Current.NavigateToUserProfile(this.Model.from, "", "", false);
    }

    private void Button_OnClicked(object sender, RoutedEventArgs e)
    {
      if (this.Model.RequestHandledAction == null)
        return;
      string format = "\r\n\r\nvar result=API.friends.{0}({{user_id:{3}}});\r\nif (({1}&&result>0)||({2}&&result.success==1)) \r\n    return API.execute.getFriendsWithRequests({{requests_count:1,requests_offset:0,without_friends:1,requests_only:{4},suggested_only:{5}}});\r\nreturn 0;";
      object[] objArray = new object[6]
      {
        (object) (sender == this.AddButton ? "add" : "delete"),
        (object) (sender == this.AddButton ? "true" : "false"),
        (object) (sender == this.AddButton ? "false" : "true"),
        (object) this.Model.user_id,
        null,
        null
      };
      int index1 = 4;
      bool? isSuggestedFriend;
      string str1;
      if (this.NeedBottomSeparatorLine)
      {
        isSuggestedFriend = this.IsSuggestedFriend;
        bool flag = false;
        if ((isSuggestedFriend.GetValueOrDefault() == flag ? (isSuggestedFriend.HasValue ? 1 : 0) : 0) != 0)
        {
          str1 = "1";
          goto label_5;
        }
      }
      str1 = "0";
label_5:
      objArray[index1] = (object) str1;
      int index2 = 5;
      string str2;
      if (this.NeedBottomSeparatorLine)
      {
        isSuggestedFriend = this.IsSuggestedFriend;
        if (isSuggestedFriend.Value)
        {
          str2 = "1";
          goto label_9;
        }
      }
      str2 = "0";
label_9:
      objArray[index2] = (object) str2;
      string str3 = string.Format(format, objArray);
      FriendRequest model = this.Model;
      Action<BackendResult<FriendRequests, ResultCode>> action = (Action<BackendResult<FriendRequests, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          FriendRequests resultData = result.ResultData;
          model.RequestHandledAction(resultData);
          CountersManager.Current.Counters.friends = resultData.menu_counter;
          EventAggregator.Current.Publish((object) new CountersChanged(CountersManager.Current.Counters));
        }
        PageBase.SetInProgress(false);
      })));
      PageBase.SetInProgress(true);
      string methodName = "execute";
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("code", str3);
      Action<BackendResult<FriendRequests, ResultCode>> callback = action;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.DispatchRequestToVK<FriendRequests>(methodName, parameters, callback, null, num1 != 0, num2 != 0, cancellationToken);
    }

    private void RecommenderName_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void Button_OnTapped(object sender, GestureEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/FriendRequestUC.xaml", UriKind.Relative));
      this.RequestPhoto = (Image) this.FindName("RequestPhoto");
      this.RequestName = (TextBlock) this.FindName("RequestName");
      this.RequestOccupation = (TextBlock) this.FindName("RequestOccupation");
      this.RequestMessage = (TextBlock) this.FindName("RequestMessage");
      this.RecommenderPanel = (Grid) this.FindName("RecommenderPanel");
      this.RecommenderName = (TextBlock) this.FindName("RecommenderName");
      this.MutualFriendsPanel = (Grid) this.FindName("MutualFriendsPanel");
      this.MutualFriendsCountBlock = (TextBlock) this.FindName("MutualFriendsCountBlock");
      this.MutualFriendsPhotosPanel = (StackPanel) this.FindName("MutualFriendsPhotosPanel");
      this.AddButton = (Button) this.FindName("AddButton");
      this.HideButton = (Button) this.FindName("HideButton");
      this.BottomSeparatorRectangle = (Rectangle) this.FindName("BottomSeparatorRectangle");
    }
  }
}
