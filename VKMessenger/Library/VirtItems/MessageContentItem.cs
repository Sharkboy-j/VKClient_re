using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Utils;

namespace VKMessenger.Library.VirtItems
{
  public class MessageContentItem : VirtualizableItemBase
  {
    private static readonly double MIN_WIDTH = 150.0;
    private static readonly int MAX_LEVEL = 4;
    private static readonly int MARGIN_BETWEEN = 8;
    private double _marginLeft = 12.0;
    private double _marginTop = 6.0;
    private MessageViewModel _mvm;
    private double _height;
    private bool _isHorizontalOrientation;
    private double _verticalWidth;
    private double _horizontalWidth;
    private NewsTextItem _textItem;
    private MessageFooterItem _messageFooterItem;
    private AttachmentsItem _attachmentsItem;
    private bool _handledDelivered;
    private int _lvl;
    private ForwardedHeaderItem _forwardedHeaderItem;
    private List<MessageContentItem> _forwardedList;
    private const bool _isFriendsOnly = false;
    private const bool _isCommentAttachments = false;
    private const bool _isMessage = true;

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
        this.Width = this._isHorizontalOrientation ? this._horizontalWidth : this._verticalWidth;
        this.GenerateLayout();
      }
    }

    private double ForwardedMarginLeft
    {
      get
      {
        return this._mvm.IsForwarded ? 0.0 : 0.0;
      }
    }

    public override double FixedHeight
    {
      get
      {
        return this._height;
      }
    }

    public MessageContentItem(double verticalWidth, Thickness margin, MessageViewModel mvm, double horizontalWidth, bool isHorizontalOrientation, int lvl = 0)
      : base(verticalWidth, margin, new Thickness())
    {
      this._mvm = mvm;
      this._lvl = lvl;
      this._verticalWidth = verticalWidth;
      this._horizontalWidth = horizontalWidth;
      this._isHorizontalOrientation = isHorizontalOrientation;
      this.Width = isHorizontalOrientation ? horizontalWidth : verticalWidth;
      this.GenerateLayout();
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
      if (!this._mvm.IsForwarded)
        return;
      Rectangle rect = new Rectangle();
      SolidColorBrush solidColorBrush = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
      rect.Fill = (Brush) solidColorBrush;
      double num1 = 0.5;
      rect.Opacity = num1;
      double num2 = 3.0;
      rect.Width = num2;
      Thickness thickness = new Thickness(0.0, this._marginTop, 0.0, 0.0);
      rect.Margin = thickness;
      double num3 = this._height - this._marginTop;
      rect.Height = num3;
      foreach (FrameworkElement coverByRectangle in RectangleHelper.CoverByRectangles(rect))
        this.Children.Add(coverByRectangle);
    }

    protected override void ReleaseResourcesOnUnload()
    {
      base.ReleaseResourcesOnUnload();
      this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
    }

    private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "UIStatusDelivered") || this._mvm.UIStatusDelivered != Visibility.Visible || this._handledDelivered)
        return;
      AttachmentsItem attachmentsItem = this.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (vc => vc is AttachmentsItem)) as AttachmentsItem;
      if (attachmentsItem != null)
      {
        ThumbsItem thumbsItem = attachmentsItem.VirtualizableChildren.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (vc => vc is ThumbsItem)) as ThumbsItem;
        if (thumbsItem != null)
        {
          VirtualizableState currentState = thumbsItem.CurrentState;
          thumbsItem.ChangeState(VirtualizableState.Unloaded);
          thumbsItem.PrepareThumbsList();
          thumbsItem.ChangeState(currentState);
        }
      }
      this._handledDelivered = true;
    }

    private void GenerateLayout()
    {
      double top = this._marginTop;
      if (this._mvm.IsForwarded)
      {
        if (this._forwardedHeaderItem == null)
        {
          this._forwardedHeaderItem = new ForwardedHeaderItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0), this._mvm);
          this.VirtualizableChildren.Add((IVirtualizable) this._forwardedHeaderItem);
        }
        top = top + this._forwardedHeaderItem.FixedHeight + (double) MessageContentItem.MARGIN_BETWEEN;
      }
      if (!string.IsNullOrWhiteSpace(this._mvm.Message.body))
      {
        if (this._textItem == null)
        {
          this._textItem = new NewsTextItem(this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0), this._mvm.Message.body, false, null, 24.0, new FontFamily("Segoe WP SemiLight"), 30.0, (Brush) (Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush), this._isHorizontalOrientation, this._horizontalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft, HorizontalAlignment.Left, "", TextAlignment.Left, true);
          this.VirtualizableChildren.Add((IVirtualizable) this._textItem);
        }
        this._textItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
        this._textItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0);
        top += this._textItem.FixedHeight;
      }
      if (this._mvm.Attachments != null && this._mvm.Attachments.Count > 0)
      {
        if (!string.IsNullOrWhiteSpace(this._mvm.Message.body))
          top += (double) MessageContentItem.MARGIN_BETWEEN;
        Geo geo = this._mvm.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (a => a.Geo != null)).Select<AttachmentViewModel, Geo>((Func<AttachmentViewModel, Geo>) (a => a.Geo)).FirstOrDefault<Geo>();
        List<Attachment> list = this._mvm.Attachments.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (a => a.Attachment != null)).Select<AttachmentViewModel, Attachment>((Func<AttachmentViewModel, Attachment>) (a => a.Attachment)).ToList<Attachment>();
        if (this._attachmentsItem == null)
        {
          double width = this._verticalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft;
          double horizontalWidth = this._horizontalWidth - this._marginLeft * 2.0 - this.ForwardedMarginLeft;
          if (width > MessageContentItem.MIN_WIDTH)
          {
            string itemId = this._mvm.Message.from_id != 0L ? this._mvm.Message.from_id.ToString() : "";
            this._attachmentsItem = new AttachmentsItem(width, new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0), list, geo, itemId, false, false, true, this._isHorizontalOrientation, horizontalWidth, this._mvm.Message.@out == 1, false, "");
            this.VirtualizableChildren.Add((IVirtualizable) this._attachmentsItem);
          }
        }
        if (this._attachmentsItem != null)
        {
          this._attachmentsItem.IsHorizontal = this.IsHorizontalOrientation;
          this._attachmentsItem.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0);
          top += this._attachmentsItem.FixedHeight;
        }
      }
      if (this._mvm.ForwardedMessages != null && this._mvm.ForwardedMessages.Count > 0)
      {
        if (this._textItem != null || this._attachmentsItem != null)
          top += (double) MessageContentItem.MARGIN_BETWEEN;
        if (this._forwardedList == null)
        {
          this._forwardedList = new List<MessageContentItem>();
          foreach (MessageViewModel forwardedMessage in (Collection<MessageViewModel>) this._mvm.ForwardedMessages)
          {
            double verticalWidth = this._verticalWidth - this._marginLeft - this.ForwardedMarginLeft;
            double horizontalWidth = this._horizontalWidth - this._marginLeft - this.ForwardedMarginLeft;
            if (verticalWidth > MessageContentItem.MIN_WIDTH && this._lvl < MessageContentItem.MAX_LEVEL)
            {
              MessageContentItem messageContentItem = new MessageContentItem(verticalWidth, new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0), forwardedMessage, horizontalWidth, this.IsHorizontalOrientation, this._lvl + 1);
              this.VirtualizableChildren.Add((IVirtualizable) messageContentItem);
              this._forwardedList.Add(messageContentItem);
              top += messageContentItem.FixedHeight;
              top += (double) MessageContentItem.MARGIN_BETWEEN;
            }
          }
        }
        else
        {
          foreach (MessageContentItem forwarded in this._forwardedList)
          {
            forwarded.IsHorizontalOrientation = this.IsHorizontalOrientation;
            forwarded.Margin = new Thickness(this._marginLeft + this.ForwardedMarginLeft, top, 0.0, 0.0);
            top += forwarded.FixedHeight;
            top += (double) MessageContentItem.MARGIN_BETWEEN;
          }
        }
        if (this._forwardedList.Any<MessageContentItem>())
          top -= (double) MessageContentItem.MARGIN_BETWEEN;
      }
      if (!this._mvm.IsForwarded)
      {
        if (this._messageFooterItem == null)
        {
          this._messageFooterItem = new MessageFooterItem(this._verticalWidth - this._marginLeft * 2.0, new Thickness(this._marginLeft, top, 0.0, 0.0), this._mvm, this.IsHorizontalOrientation, this._horizontalWidth - this._marginLeft * 2.0);
          this.VirtualizableChildren.Add((IVirtualizable) this._messageFooterItem);
        }
        else
        {
          this._messageFooterItem.IsHorizontalOrientation = this.IsHorizontalOrientation;
          this._messageFooterItem.Margin = new Thickness(this._marginLeft, top, 0.0, 0.0);
        }
        top += this._messageFooterItem.FixedHeight + (double) MessageContentItem.MARGIN_BETWEEN;
      }
      if (!this._mvm.IsForwarded)
        top += this._marginTop;
      this._height = top;
    }
  }
}
