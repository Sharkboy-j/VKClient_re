using Microsoft.Phone.Controls;
using System;
//using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Localization;

namespace VKMessenger.Library.VirtItems
{
    public class MessageFooterItem : VirtualizableItemBase
    {
        private StackPanel _stackPanelCannotSend;
        private ProgressBar _progressBar;
        private Grid _gridDateStatus;
        private StackPanel _stackPanelDateStatus;
        private TextBlock _dateTextBlock;
        private MessageViewModel _mvm;
        private double _verticalWidth;
        private double _horizontalWidth;
        private bool _isHorizontalOrientation;
        private Border _borderStatus;

        public bool IsHorizontalOrientation
        {
            get
            {
                return this._isHorizontalOrientation;
            }
            set
            {
                if (this._isHorizontalOrientation == value)
                    return;
                this._isHorizontalOrientation = value;
                this.UpdateLayout();
            }
        }

        public bool IsSticker
        {
            get
            {
                if (this._mvm.Attachments != null)
                    return this._mvm.Attachments.Any<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a => a.AttachmentType == AttachmentType.Sticker));
                return false;
            }
        }

        // NEW: 4.8.0
        private bool IsNotStickerOrGraffiti
        {
            get
            {
                return this._mvm.Attachments.All<AttachmentViewModel>((Func<AttachmentViewModel, bool>)(a =>
                {
                    if (a.AttachmentType == AttachmentType.Sticker || a.AttachmentType == AttachmentType.Document)
                        return false;
                    Attachment attachment = a.Attachment;
                    bool? nullable;
                    if (attachment == null)
                    {
                        nullable = new bool?();
                    }
                    else
                    {
                        Doc doc = attachment.doc;
                        nullable = doc != null ? new bool?(doc.IsGraffiti) : new bool?();
                    }
                    return nullable ?? false;
                }));
            }
        }

        public Brush ForegroundBrush
        {
            get
            {
                return Application.Current.Resources["PhoneVKSubtleBrush"] as Brush;
            }
        }

        public override double FixedHeight
        {
            get
            {
                return 20.0;
            }
        }

        public MessageFooterItem(double width, Thickness margin, MessageViewModel mvm, bool isHorizontalOrientation, double horizontalWidth)
            : base(width, margin, new Thickness())
        {
            this._mvm = mvm;
            this._verticalWidth = width;
            this._horizontalWidth = horizontalWidth;
            this._isHorizontalOrientation = isHorizontalOrientation;
            this.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
            this.CreateLayout();
        }

        private new void UpdateLayout()
        {
            this.Width = this.IsHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
            this._gridDateStatus.Width = this.Width;
        }

        private void CreateLayout()// UPDTE: 4.8.0
        {
            this._stackPanelCannotSend = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };
            UIElementCollection children = this._stackPanelCannotSend.Children;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = CommonResources.Conversation_MessageWasNotSent;
            textBlock.FontSize = 20.0;
            FontFamily fontFamily = new FontFamily("Segoe WP");
            textBlock.FontFamily = fontFamily;
            Brush foregroundBrush1 = this.ForegroundBrush;
            textBlock.Foreground = foregroundBrush1;
            children.Add((UIElement)textBlock);
            HyperlinkButton hyperlinkButton1 = new HyperlinkButton();
            double num1 = 20.0;
            hyperlinkButton1.FontSize = num1;
            string conversationRetry = CommonResources.Conversation_Retry;
            hyperlinkButton1.Content = (object)conversationRetry;
            Brush foregroundBrush2 = this.ForegroundBrush;
            hyperlinkButton1.Foreground = foregroundBrush2;
            HyperlinkButton hyperlinkButton2 = hyperlinkButton1;
            hyperlinkButton2.Click += new RoutedEventHandler(this.hb_Click);
            this._stackPanelCannotSend.Children.Add((UIElement)hyperlinkButton2);
            this._gridDateStatus = new Grid();
            this._gridDateStatus.Width = this.Width;
            this._gridDateStatus.Height = this.FixedHeight + 8.0;
            this._gridDateStatus.ColumnDefinitions.Add(new ColumnDefinition());
            this._gridDateStatus.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto
            });
            this._stackPanelDateStatus = new StackPanel();
            this._stackPanelDateStatus.Orientation = Orientation.Horizontal;
            if (this._mvm.Attachments == null || this.IsNotStickerOrGraffiti || this._mvm.Message.@out == 1)
            {
                this._stackPanelDateStatus.HorizontalAlignment = HorizontalAlignment.Right;
                Grid.SetColumn((FrameworkElement)this._stackPanelDateStatus, 1);
            }
            this._gridDateStatus.Children.Add((UIElement)this._stackPanelDateStatus);
            if (this._mvm.Message.@out == 1)
            {
                ProgressBar progressBar = new ProgressBar();
                double num2 = 100.0;
                progressBar.Maximum = num2;
                this._progressBar = progressBar;
                this._progressBar.Margin = new Thickness(-12.0, 3.0, 0.0, 0.0);
                this._progressBar.IsIndeterminate = false;
                this._progressBar.Visibility = Visibility.Collapsed;
                this._progressBar.Foreground = this.ForegroundBrush;
                this._progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
                this._gridDateStatus.Children.Add((UIElement)this._progressBar);
            }
            this._dateTextBlock = new TextBlock();
            this._dateTextBlock.Opacity = this._mvm.TextOpacity;
            this._dateTextBlock.Text = this._mvm.UIDate;
            this._dateTextBlock.FontSize = 20.0;
            this._dateTextBlock.FontFamily = new FontFamily("Segoe WP");
            this._dateTextBlock.Foreground = this.ForegroundBrush;
            this._borderStatus = new Border();
            this._borderStatus.Background = this.ForegroundBrush;
            this._borderStatus.Width = 21.0;
            this._borderStatus.Height = 18.0;
            this._borderStatus.Margin = new Thickness(5.0, 0.0, 0.0, 0.0);
            this._stackPanelDateStatus.Children.Add((UIElement)this._dateTextBlock);
            this._stackPanelDateStatus.Children.Add((UIElement)this._borderStatus);
        }

        private void hb_Click(object sender, RoutedEventArgs e)
        {
            PhoneApplicationPage phoneApplicationPage = ((ContentControl)Application.Current.RootVisual).Content as PhoneApplicationPage;
            if (phoneApplicationPage != null)
                phoneApplicationPage.Focus();
            this._mvm.Send();
        }

        protected override void GenerateChildren()
        {
            base.GenerateChildren();
            this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
            if (this._mvm.OutboundMessageVM != null)
                this._mvm.OutboundMessageVM.PropertyChanged += new PropertyChangedEventHandler(this.OutboundMessageVM_PropertyChanged);
            this.UpdateState();
        }

        private void OutboundMessageVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "UploadProgress"))
                return;
            this.UpdateProgress();
        }

        private void UpdateState()
        {
            this.UpdateProgress();
            if (this._mvm.Message.@out == 1 && this._mvm.SendStatus == OutboundMessageStatus.Failed)
            {
                if (!this._view.Children.Contains((UIElement)this._stackPanelCannotSend))
                    this._view.Children.Add((UIElement)this._stackPanelCannotSend);
                if (this._view.Children.Contains((UIElement)this._gridDateStatus))
                    this._view.Children.Remove((UIElement)this._gridDateStatus);
            }
            else
            {
                if (this._view.Children.Contains((UIElement)this._stackPanelCannotSend))
                    this._view.Children.Remove((UIElement)this._stackPanelCannotSend);
                if (!this._view.Children.Contains((UIElement)this._gridDateStatus))
                    this._view.Children.Add((UIElement)this._gridDateStatus);
            }
            this._borderStatus.Visibility = this._mvm.Message.@out == 1 ? Visibility.Visible : Visibility.Collapsed;
            if (this._mvm.Message.@out != 1)
                return;
            this._borderStatus.OpacityMask = (Brush)new ImageBrush()
            {
                ImageSource = (ImageSource)new BitmapImage(new Uri(this._mvm.StatusImage, UriKind.Relative))
                {
                    CreateOptions = (BitmapCreateOptions.DelayCreation | BitmapCreateOptions.BackgroundCreation)
                }
            };
        }

        private void UpdateProgress()
        {
            if (this._progressBar == null || this._mvm.OutboundMessageVM == null || this._mvm.OutboundMessageVM.CountUploadableAttachments <= 0)
                return;
            this._progressBar.Visibility = this._mvm.SendStatus == OutboundMessageStatus.SendingNow ? Visibility.Visible : Visibility.Collapsed;
            this._progressBar.Animate(this._progressBar.Value, this._mvm.OutboundMessageVM.UploadProgress, (object)RangeBase.ValueProperty, 100, new int?(), (IEasingFunction)null, null);
        }

        private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UIStatusDelivered")
            {
                VirtualizableState currentState = this.CurrentState;
                this.Unload();
                this.Load(currentState);
            }
            if (!(e.PropertyName == "UIDate"))
                return;
            this._dateTextBlock.Text = this._mvm.UIDate;
        }

        protected override void ReleaseResourcesOnUnload()
        {
            base.ReleaseResourcesOnUnload();
            this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
            if (this._mvm.OutboundMessageVM != null)
                this._mvm.OutboundMessageVM.PropertyChanged -= new PropertyChangedEventHandler(this.OutboundMessageVM_PropertyChanged);
            this._borderStatus.ClearValue(UIElement.OpacityMaskProperty);
        }
    }
}
