using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Audio.Base.BackendServices;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class NewsfeedNotificationUC : UserControlVirtualizable
    {
        private const double TEXT_MARGIN_LEFT_RIGHT_LARGE = 24.0;
        private UserNotification _userNotification;
        private UserNotificationNewsfeed _newsfeed;
        private double _width;
        private Action _hideCallback;
        private double _height;
        private List<UserNotificationImage> _images;
        private string _title;
        private string _message;
        private UserNotificationButton _button;
        private List<User> _users;
        private List<Group> _groups;
        private string _usersDescription;
        private string _groupsDescription;
        private bool _hasTitle;
        private bool _hasMessage;
        private bool _hasImages;
        private bool _hasButton;
        private bool _hasUsers;
        private bool _hasGroups;
        private bool _hasUsersDescription;
        private bool _hasGroupsDescription;
        private string _navigationUrl;
        private Uri _imageUri;
        private Image _image;
        private Canvas _imageContainer;
        private double _fixedHeight;
        private const double HIDE_BUTTON_WIDTH_HEIGHT = 56.0;
        private bool _isNavigating;
        internal StackPanel stackPanel;
        internal Canvas canvasDismiss;
        private bool _contentLoaded;

        public NewsfeedNotificationUC()
        {
            this.InitializeComponent();
        }

        public void Initialize(UserNotification userNotification, List<User> users, List<Group> groups, double width, Action hideCallback)
        {
            this._userNotification = userNotification;
            this._users = users;
            this._groups = groups;
            this._width = width;
            this._hideCallback = hideCallback;
            this._newsfeed = this._userNotification.newsfeed;
            this._images = this._newsfeed.images;
            this._title = this._newsfeed.title;
            this._message = this._newsfeed.message;
            this._button = this._newsfeed.button;
            this._usersDescription = this._newsfeed.users_description;
            this._groupsDescription = this._newsfeed.groups_description;
            this._hasTitle = !string.IsNullOrWhiteSpace(this._title);
            this._hasMessage = !string.IsNullOrWhiteSpace(this._message);
            List<UserNotificationImage> notificationImageList = this._images;
            this._hasImages = notificationImageList != null && (notificationImageList.Count) > 0;
            UserNotificationButton notificationButton1 = this._button;
            this._hasButton = !string.IsNullOrWhiteSpace(notificationButton1 != null ? notificationButton1.title : null) && this._button.action != null;
            List<User> userList = this._users;
            this._hasUsers = userList != null && (userList.Count) > 0;
            List<Group> groupList = this._groups;
            this._hasGroups = groupList != null && (groupList.Count) > 0;
            this._hasUsersDescription = !string.IsNullOrWhiteSpace(this._usersDescription);
            this._hasGroupsDescription = !string.IsNullOrWhiteSpace(this._groupsDescription);
            UserNotificationButton notificationButton2 = this._button;
            int num1;
            if (notificationButton2 == null)
            {
                num1 = 0;
            }
            else
            {
                UserNotificationButtonAction action = notificationButton2.action;
                UserNotificationButtonActionType? nullable = action != null ? new UserNotificationButtonActionType?(action.type) : new UserNotificationButtonActionType?();
                UserNotificationButtonActionType buttonActionType = UserNotificationButtonActionType.open_url;
                num1 = nullable.GetValueOrDefault() == buttonActionType ? (nullable.HasValue ? 1 : 0) : 0;
            }
            if (num1 != 0)
                this._navigationUrl = this._button.action.url;
            this.stackPanel.Children.Clear();
            this.stackPanel.Width = width;
            this._height = 0.0;
            if (this._newsfeed.layout == UserNotificationNewsfeedLayout.banner)
                this.ComposeLargeUI();
            else
                this.ComposeSmallMediumUI();
            Rectangle rectangle1 = new Rectangle();
            double num2 = 16.0;
            rectangle1.Height = num2;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneNewsDividerBrush"];
            rectangle1.Fill = (Brush)solidColorBrush;
            Rectangle rectangle2 = rectangle1;
            this.stackPanel.Children.Add((UIElement)rectangle2);
            this._height = this._height + rectangle2.Height;
            this._fixedHeight = Math.Max(this._height, this.canvasDismiss.Height);
        }

        private void ComposeLargeUI()
        {
            if (this._hasImages)
            {
                Canvas image = this.GetImage();
                this.stackPanel.Children.Add((UIElement)image);
                this._height = this._height + image.Height;
            }
            Thickness margin;
            if (this._hasTitle)
            {
                TextBlock titleTextBlockLarge = this.GetTitleTextBlockLarge(this._hasImages ? 16.0 : 24.0);
                this.stackPanel.Children.Add((UIElement)titleTextBlockLarge);
                double num1 = this._height;
                margin = titleTextBlockLarge.Margin;
                double num2 = margin.Top + titleTextBlockLarge.ActualHeight;
                margin = titleTextBlockLarge.Margin;
                double bottom = margin.Bottom;
                double num3 = num2 + bottom;
                this._height = num1 + num3;
            }
            if (this._hasMessage)
            {
                TextBlock messageTextBlockLarge = this.GetMessageTextBlockLarge(!this._hasTitle ? (this._hasImages ? 16.0 : 24.0) : 10.0);
                this.stackPanel.Children.Add((UIElement)messageTextBlockLarge);
                double num1 = this._height;
                margin = messageTextBlockLarge.Margin;
                double num2 = margin.Top + messageTextBlockLarge.ActualHeight;
                margin = messageTextBlockLarge.Margin;
                double bottom = margin.Bottom;
                double num3 = num2 + bottom;
                this._height = num1 + num3;
            }
            if (this._hasUsers || this._hasGroups)
            {
                ItemsControl usersGroupsListLarge = this.GetUsersGroupsListLarge(24.0);
                if (usersGroupsListLarge != null)
                {
                    this.stackPanel.Children.Add((UIElement)usersGroupsListLarge);
                    double num1 = this._height;
                    margin = usersGroupsListLarge.Margin;
                    double num2 = margin.Top + usersGroupsListLarge.Height;
                    margin = usersGroupsListLarge.Margin;
                    double bottom = margin.Bottom;
                    double num3 = num2 + bottom;
                    this._height = num1 + num3;
                }
            }
            if (this._hasUsersDescription || this._hasGroupsDescription)
            {
                TextBlock descriptionTextBlockLarge = this.GetUsersGroupsDescriptionTextBlockLarge(this._hasUsers || this._hasGroups ? 8.0 : 24.0);
                this.stackPanel.Children.Add((UIElement)descriptionTextBlockLarge);
                double num1 = this._height;
                margin = descriptionTextBlockLarge.Margin;
                double num2 = margin.Top + descriptionTextBlockLarge.ActualHeight;
                margin = descriptionTextBlockLarge.Margin;
                double bottom = margin.Bottom;
                double num3 = num2 + bottom;
                this._height = num1 + num3;
            }
            if (this._hasButton)
            {
                FrameworkElement button = this.GetButton(this._width - 32.0, 24.0, 0.0);
                button.HorizontalAlignment = HorizontalAlignment.Center;
                this.stackPanel.Children.Add((UIElement)button);
                double num1 = this._height;
                margin = button.Margin;
                double num2 = margin.Top + button.Height;
                margin = button.Margin;
                double bottom = margin.Bottom;
                double num3 = num2 + bottom;
                this._height = num1 + num3;
            }
            if (this._hasButton && this._button.style == UserNotificationButtonStyle.cell)
                return;
            UIElementCollection children = this.stackPanel.Children;
            Rectangle rectangle = new Rectangle();
            double num = 24.0;
            rectangle.Height = num;
            children.Add((UIElement)rectangle);
            this._height = this._height + 24.0;
        }

        private Canvas GetImage()
        {
            double num1;
            double num2;
            switch (this._newsfeed.layout)
            {
                case UserNotificationNewsfeedLayout.info:
                    num2 = num1 = 44.0;
                    break;
                case UserNotificationNewsfeedLayout.app:
                    num2 = num1 = 80.0;
                    break;
                default:
                    UserNotificationImage notificationImage1 = this._images[0];
                    double num3 = (double)notificationImage1.width;
                    double num4 = (double)notificationImage1.height;
                    if (num3 > 0.0 && num4 > 0.0)
                    {
                        double num5 = num3 / num4;
                        num2 = this._width;
                        num1 = num2 / num5;
                        break;
                    }
                    num2 = num1 = this._width;
                    break;
            }
            Image image = new Image();
            double num6 = num2;
            image.Width = num6;
            double num7 = num1;
            image.Height = num7;
            int num8 = 3;
            image.Stretch = (Stretch)num8;
            int num9 = 1;
            image.HorizontalAlignment = (HorizontalAlignment)num9;
            int num10 = 1;
            image.VerticalAlignment = (VerticalAlignment)num10;
            this._image = image;
            double num11 = (double)ScaleFactor.GetRealScaleFactor() / 100.0;
            int scaledImageWidth = (int)Math.Round(num2 * num11);
            int scaledImageHeight = (int)Math.Round(num1 * num11);
            UserNotificationImage notificationImage2 = this._images.FirstOrDefault<UserNotificationImage>((Func<UserNotificationImage, bool>)(i =>
            {
                if (i.width >= scaledImageWidth)
                    return i.height >= scaledImageHeight;
                return false;
            }));
            this._imageUri = ((notificationImage2 != null ? notificationImage2.url : null) ?? this._images.Last<UserNotificationImage>().url).ConvertToUri();
            Canvas canvas = new Canvas();
            double width = this._image.Width;
            canvas.Width = width;
            double height = this._image.Height;
            canvas.Height = height;
            this._imageContainer = canvas;
            this._imageContainer.Children.Add((UIElement)this._image);
            return this._imageContainer;
        }

        private TextBlock GetTitleTextBlockLarge(double marginTop)
        {
            TextBlock textBlock = new TextBlock();
            double num1 = this._width - 48.0;
            textBlock.Width = num1;
            Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
            textBlock.Margin = thickness;
            double num2 = 25.33;
            textBlock.FontSize = num2;
            int num3 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num3;
            double num4 = 32.0;
            textBlock.LineHeight = num4;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneAlmostBlackBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            int num5 = 0;
            textBlock.TextAlignment = (TextAlignment)num5;
            int num6 = 2;
            textBlock.TextWrapping = (TextWrapping)num6;
            string str = this._title;
            textBlock.Text = str;
            return textBlock;
        }

        private TextBlock GetMessageTextBlockLarge(double marginTop)
        {
            TextBlock textBlock = new TextBlock();
            double num1 = this._width - 48.0;
            textBlock.Width = num1;
            Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
            textBlock.Margin = thickness;
            double num2 = 20.0;
            textBlock.FontSize = num2;
            int num3 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num3;
            double num4 = 24.0;
            textBlock.LineHeight = num4;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneDarkGrayBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            int num5 = 0;
            textBlock.TextAlignment = (TextAlignment)num5;
            int num6 = 2;
            textBlock.TextWrapping = (TextWrapping)num6;
            string str = this._message;
            textBlock.Text = str;
            return textBlock;
        }

        private ItemsControl GetUsersGroupsListLarge(double marginTop)
        {
            ItemsControl usersGroupsList = this.GetUsersGroupsList();
            if (usersGroupsList != null)
            {
                usersGroupsList.Margin = new Thickness(0.0, marginTop, 0.0, 0.0);
                usersGroupsList.HorizontalAlignment = HorizontalAlignment.Center;
                usersGroupsList.VerticalAlignment = VerticalAlignment.Top;
            }
            return usersGroupsList;
        }

        private TextBlock GetUsersGroupsDescriptionTextBlockLarge(double marginTop)
        {
            string str1;
            if (this._hasUsersDescription)
            {
                str1 = this._usersDescription;
            }
            else
            {
                if (!this._hasGroupsDescription)
                    return (TextBlock)null;
                str1 = this._groupsDescription;
            }
            TextBlock textBlock = new TextBlock();
            double num1 = this._width - 48.0;
            textBlock.Width = num1;
            Thickness thickness = new Thickness(24.0, marginTop, 24.0, 0.0);
            textBlock.Margin = thickness;
            double num2 = 18.0;
            textBlock.FontSize = num2;
            int num3 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num3;
            double num4 = 22.0;
            textBlock.LineHeight = num4;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneDarkGrayBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            int num5 = 0;
            textBlock.TextAlignment = (TextAlignment)num5;
            int num6 = 2;
            textBlock.TextWrapping = (TextWrapping)num6;
            string str2 = str1;
            textBlock.Text = str2;
            return textBlock;
        }

        private void ComposeSmallMediumUI()
        {
            Canvas canvas1 = new Canvas();
            double num1 = this._width;
            canvas1.Width = num1;
            SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Transparent);
            canvas1.Background = (Brush)solidColorBrush;
            int num2 = 0;
            canvas1.VerticalAlignment = (VerticalAlignment)num2;
            Canvas canvas2 = canvas1;
            double val1 = 0.0;
            double num3 = 0.0;
            if (this._hasImages)
            {
                Canvas image = this.GetImage();
                Canvas.SetLeft((UIElement)image, 16.0);
                Canvas.SetTop((UIElement)image, 16.0);
                canvas2.Children.Add((UIElement)image);
                val1 = 16.0 + image.Height;
                num3 = 16.0 + image.Width;
            }
            double val2 = 0.0;
            double length = 16.0;
            double num4 = 4.0;
            if (this._newsfeed.layout == UserNotificationNewsfeedLayout.info)
            {
                length = 12.0;
                num4 = 8.0;
            }
            double num5 = Math.Max(16.0, num3 + 16.0);
            double num6 = this._width - num5 - 56.0 - 8.0;
            if (this._hasTitle)
            {
                TextBlock blockMediumSmall = this.GetTitleTextBlockMediumSmall(num6);
                Canvas.SetTop((UIElement)blockMediumSmall, length);
                Canvas.SetLeft((UIElement)blockMediumSmall, num5);
                canvas2.Children.Add((UIElement)blockMediumSmall);
                val2 += Canvas.GetTop((UIElement)blockMediumSmall) + blockMediumSmall.ActualHeight;
            }
            if (this._hasMessage)
            {
                TextBlock blockMediumSmall = this.GetMessageTextBlockMediumSmall(num6);
                double num7 = this._hasTitle ? num4 : length;
                Canvas.SetTop((UIElement)blockMediumSmall, val2 + num7);
                Canvas.SetLeft((UIElement)blockMediumSmall, num5);
                canvas2.Children.Add((UIElement)blockMediumSmall);
                val2 += num7 + blockMediumSmall.ActualHeight;
            }
            this._height = Math.Max(val1, val2);
            canvas2.Height = this._height;
            this.stackPanel.Children.Add((UIElement)canvas2);
            StackPanel stackPanel1 = new StackPanel();
            Thickness thickness = new Thickness(16.0, 8.0, 0.0, 0.0);
            stackPanel1.Margin = thickness;
            double num8 = 64.0;
            stackPanel1.Height = num8;
            int num9 = 0;
            stackPanel1.HorizontalAlignment = (HorizontalAlignment)num9;
            int num10 = 1;
            stackPanel1.Orientation = (Orientation)num10;
            StackPanel stackPanel2 = stackPanel1;
            ItemsControl itemsControl = (ItemsControl)null;
            if (this._hasUsers || this._hasGroups)
            {
                itemsControl = this.GetUsersGroupsListMediumSmall(12.0);
                if (itemsControl != null)
                    stackPanel2.Children.Add((UIElement)itemsControl);
            }
            if (this._hasUsersDescription || this._hasGroupsDescription)
            {
                TextBlock blockMediumSmall = this.GetUsersGroupsDescriptionTextBlockMediumSmall(itemsControl != null ? itemsControl.Width : 0.0);
                stackPanel2.Children.Add((UIElement)blockMediumSmall);
            }
            bool flag = stackPanel2.Children.Count > 0;
            Thickness margin;
            if (flag)
            {
                this.stackPanel.Children.Add((UIElement)stackPanel2);
                double num7 = this._height;
                double num11;
                if (stackPanel2.Children.Count <= 0)
                {
                    num11 = 0.0;
                }
                else
                {
                    double height = stackPanel2.Height;
                    margin = stackPanel2.Margin;
                    double top = margin.Top;
                    num11 = height + top;
                }
                this._height = num7 + num11;
            }
            if (this._hasButton)
            {
                double marginTop = flag ? 8.0 : 20.0;
                FrameworkElement button = this.GetButton(num6, marginTop, num5);
                button.HorizontalAlignment = HorizontalAlignment.Left;
                this.stackPanel.Children.Add((UIElement)button);
                double num7 = this._height;
                margin = button.Margin;
                double num11 = margin.Top + button.Height;
                margin = button.Margin;
                double bottom = margin.Bottom;
                double num12 = num11 + bottom;
                this._height = num7 + num12;
            }
            if (this._hasButton && this._button.style == UserNotificationButtonStyle.cell)
                return;
            UIElementCollection children = this.stackPanel.Children;
            Rectangle rectangle = new Rectangle();
            double num13 = 20.0;
            rectangle.Height = num13;
            children.Add((UIElement)rectangle);
            this._height = this._height + 20.0;
        }

        private TextBlock GetTitleTextBlockMediumSmall(double width)
        {
            TextBlock textBlock = new TextBlock();
            double num1 = width;
            textBlock.Width = num1;
            double num2 = 22.67;
            textBlock.FontSize = num2;
            int num3 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num3;
            double num4 = 26.0;
            textBlock.LineHeight = num4;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneAlmostBlackBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            int num5 = 2;
            textBlock.TextWrapping = (TextWrapping)num5;
            string str = this._title;
            textBlock.Text = str;
            return textBlock;
        }

        private TextBlock GetMessageTextBlockMediumSmall(double width)
        {
            TextBlock textBlock = new TextBlock();
            double num1 = width;
            textBlock.Width = num1;
            int num2 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num2;
            double num3 = 20.0;
            textBlock.FontSize = num3;
            double num4 = 24.0;
            textBlock.LineHeight = num4;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneDarkGrayBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            int num5 = 2;
            textBlock.TextWrapping = (TextWrapping)num5;
            string str = this._message;
            textBlock.Text = str;
            return textBlock;
        }

        private ItemsControl GetUsersGroupsListMediumSmall(double marginTop)
        {
            ItemsControl usersGroupsList = this.GetUsersGroupsList();
            if (usersGroupsList != null)
            {
                usersGroupsList.Margin = new Thickness(0.0, marginTop, 0.0, 0.0);
                usersGroupsList.VerticalAlignment = VerticalAlignment.Top;
            }
            return usersGroupsList;
        }

        private TextBlock GetUsersGroupsDescriptionTextBlockMediumSmall(double usersGroupsTotalWidth)
        {
            string str1;
            if (this._hasUsersDescription)
            {
                str1 = this._usersDescription;
            }
            else
            {
                if (!this._hasGroupsDescription)
                    return (TextBlock)null;
                str1 = this._groupsDescription;
            }
            double num1 = this._width - usersGroupsTotalWidth - 32.0;
            if (!this._hasImages && !this._hasTitle && !this._hasMessage)
                num1 -= 48.0;
            TextBlock textBlock = new TextBlock();
            double num2 = num1;
            textBlock.Width = num2;
            Thickness thickness = new Thickness(16.0, 19.0, 0.0, 0.0);
            textBlock.Margin = thickness;
            int num3 = 0;
            textBlock.VerticalAlignment = (VerticalAlignment)num3;
            double num4 = 18.0;
            textBlock.FontSize = num4;
            int num5 = 1;
            textBlock.LineStackingStrategy = (LineStackingStrategy)num5;
            double num6 = 22.0;
            textBlock.LineHeight = num6;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            SolidColorBrush solidColorBrush = (SolidColorBrush)Application.Current.Resources["PhoneDarkGrayBrush"];
            textBlock.Foreground = (Brush)solidColorBrush;
            string str2 = str1;
            textBlock.Text = str2;
            return textBlock;
        }

        private ItemsControl GetUsersGroupsList()
        {
            int count = this._newsfeed.layout == UserNotificationNewsfeedLayout.banner ? 5 : 4;
            IList list1;
            if (this._hasUsers)
            {
                list1 = (IList)this._users.Take<User>(count).ToList<User>();
            }
            else
            {
                if (!this._hasGroups)
                    return (ItemsControl)null;
                list1 = (IList)this._groups.Take<Group>(count).ToList<Group>();
            }
            ItemsControl itemsControl = new ItemsControl();
            double num1 = (double)list1.Count * 44.0 - 4.0;
            itemsControl.Width = num1;
            double num2 = 40.0;
            itemsControl.Height = num2;
            IList list2 = list1;
            itemsControl.ItemsSource = (IEnumerable)list2;
            DataTemplate dataTemplate = (DataTemplate)this.Resources["UserGroupItemTemplate"];
            itemsControl.ItemTemplate = dataTemplate;
            ItemsPanelTemplate itemsPanelTemplate = (ItemsPanelTemplate)this.Resources["HorizontalItemsPanelTemplate"];
            itemsControl.ItemsPanel = itemsPanelTemplate;
            return itemsControl;
        }

        private FrameworkElement GetButton(double maxWidth, double marginTop, double marginLeft = 0.0)
        {
            EventHandler<GestureEventArgs> eventHandler1 = (EventHandler<GestureEventArgs>)((sender, args) =>
            {
                args.Handled = true;
                switch (this._button.action.type)
                {
                    case UserNotificationButtonActionType.open_url:
                        this.HandleButtonTap();
                        break;
                    case UserNotificationButtonActionType.enable_top_newsfeed:
                        this.HandleNewsfeedPromoButton();
                        break;
                }
            });
            if (this._button.style == UserNotificationButtonStyle.cell)
            {
                Grid grid = new Grid();
                double num1 = 56.0;
                grid.Height = num1;
                double num2 = this._width;
                grid.Width = num2;
                Thickness thickness1 = new Thickness(0.0, marginTop, 0.0, 0.0);
                grid.Margin = thickness1;
                Rectangle rectangle1 = new Rectangle();
                double num3 = 1.0;
                rectangle1.Height = num3;
                Thickness thickness2 = new Thickness(16.0, 0.0, 16.0, 0.0);
                rectangle1.Margin = thickness2;
                int num4 = 0;
                rectangle1.VerticalAlignment = (VerticalAlignment)num4;
                SolidColorBrush solidColorBrush1 = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
                rectangle1.Fill = (Brush)solidColorBrush1;
                double num5 = 0.1;
                rectangle1.Opacity = num5;
                Rectangle rectangle2 = rectangle1;
                grid.Children.Add((UIElement)rectangle2);
                Border border = new Border()
                {
                    Background = (Brush)new SolidColorBrush(Colors.Transparent)
                };
                MetroInMotion.SetTilt((DependencyObject)border, 1.5);
                TextBlock textBlock1 = new TextBlock();
                Thickness thickness3 = new Thickness(0.0, 12.0, 0.0, 0.0);
                textBlock1.Margin = thickness3;
                int num6 = 0;
                textBlock1.VerticalAlignment = (VerticalAlignment)num6;
                FontFamily fontFamily = new FontFamily("Segoe WP Semibold");
                textBlock1.FontFamily = fontFamily;
                double num7 = 21.33;
                textBlock1.FontSize = num7;
                SolidColorBrush solidColorBrush2 = (SolidColorBrush)Application.Current.Resources["PhoneAccentBlueBrush"];
                textBlock1.Foreground = (Brush)solidColorBrush2;
                int num8 = 0;
                textBlock1.TextAlignment = (TextAlignment)num8;
                string title = this._button.title;
                textBlock1.Text = title;
                TextBlock textBlock2 = textBlock1;
                border.Child = (UIElement)textBlock2;
                grid.Children.Add((UIElement)border);
                EventHandler<GestureEventArgs> eventHandler2 = eventHandler1;
                grid.Tap += eventHandler2;
                return (FrameworkElement)grid;
            }
            Style style1 = (Style)Application.Current.Resources[this._button.style == UserNotificationButtonStyle.primary ? (object)"VKButtonPrimaryStyle" : (object)"VKButtonSecondaryStyle"];
            Button button = new Button();
            Thickness thickness = new Thickness(marginLeft - 12.0, marginTop - 12.0, -12.0, -12.0);
            button.Margin = thickness;
            double num9 = 68.0;
            button.Height = num9;
            double num10 = maxWidth + 24.0;
            button.MaxWidth = num10;
            Style style2 = style1;
            button.Style = style2;
            string title1 = this._button.title;
            button.Content = (object)title1;
            EventHandler<GestureEventArgs> eventHandler3 = eventHandler1;
            button.Tap += eventHandler3;
            return (FrameworkElement)button;
        }

        public double CalculateTotalHeight()
        {
            return this._fixedHeight;
        }

        private void Dismiss_OnTap(object sender, GestureEventArgs e)
        {
            this.HideNotification(NewsFeedNotificationHideReason.decline);
        }

        private void HandleNewsfeedPromoButton()
        {
            AppGlobalStateManager.Current.GlobalState.NewsfeedTopEnabled = true;
            NewsViewModel.Instance.TopFeedPromoAnswer = new bool?(true);
            NewsViewModel.Instance.TopFeedPromoId = this._userNotification.id;
            NewsViewModel.Instance.UpdateFeedType();
            this.HideNotification(NewsFeedNotificationHideReason.accept);
        }

        private async void HandleButtonTap()
        {
            if (this._isNavigating || string.IsNullOrEmpty(this._navigationUrl))
                return;
            this._isNavigating = true;
            if (this._navigationUrl.StartsWith("webview"))
            {
                this._navigationUrl = string.Format("https{0}", (object)this._navigationUrl.Substring("webview".Length));
                Navigator.Current.NavigateToWebViewPage(this._navigationUrl);
            }
            else
            {
                Navigator.Current.NavigateToWebUri(this._navigationUrl, false, false);
                await Task.Delay(300);
            }
            this.HideNotification(NewsFeedNotificationHideReason.accept);
            this._isNavigating = false;
        }

        private void HideNotification(NewsFeedNotificationHideReason reason)
        {
            InternalService.Instance.HideUserNotification(this._userNotification.id, reason, (Action<BackendResult<bool, ResultCode>>)(result => { }));
            Action action = this._hideCallback;
            if (action == null)
                return;
            action();
        }

        public override void LoadFullyNonVirtualizableItems()
        {
            if (this._image == null)
                return;
            if (this._newsfeed.layout == UserNotificationNewsfeedLayout.banner)
            {
                this._imageContainer.Background = (Brush)Application.Current.Resources["PhoneChromeBrush"];
                this._image.ImageOpened -= new EventHandler<RoutedEventArgs>(this.OnImageOpened);
                this._image.ImageOpened += new EventHandler<RoutedEventArgs>(this.OnImageOpened);
            }
            VeryLowProfileImageLoader.SetUriSource(this._image, this._imageUri);
        }

        public override void ReleaseResources()
        {
            if (this._image == null)
                return;
            VeryLowProfileImageLoader.SetUriSource(this._image, null);
            this._imageContainer.Background = (Brush)Application.Current.Resources["PhoneChromeBrush"];
        }

        public override void ShownOnScreen()
        {
            if (!(this._imageUri != null) || !this._imageUri.IsAbsoluteUri)
                return;
            VeryLowProfileImageLoader.SetPriority(this._imageUri.OriginalString, DateTime.Now.Ticks);
        }

        private void OnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this._imageContainer == null)
                return;
            this._imageContainer.Background = (Brush)new SolidColorBrush(Colors.Transparent);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/NewsfeedNotificationUC.xaml", UriKind.Relative));
            this.stackPanel = (StackPanel)this.FindName("stackPanel");
            this.canvasDismiss = (Canvas)this.FindName("canvasDismiss");
        }
    }
}
