using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKMessenger.Library.VirtItems
{
  public class ForwardedHeaderItem : VirtualizableItemBase
  {
    private MessageViewModel _mvm;

    public override double FixedHeight
    {
      get
      {
        return 40.0;
      }
    }

    public ForwardedHeaderItem(double width, Thickness margin, MessageViewModel mvm)
      : base(width, margin, new Thickness())
    {
      this._mvm = mvm;
      this.CreateLayout();
    }

    private void CreateLayout()
    {
      this.VirtualizableChildren.Add((IVirtualizable) new VirtualizableImage(40.0, 40.0, new Thickness(), this._mvm.UIImageUrl, new Action<VirtualizableImage>(this.imageTap), "1", false, true, Stretch.UniformToFill, (Brush) null, -1.0, false, true));
    }

    private void imageTap(VirtualizableImage obj)
    {
      long uid = this._mvm.AssociatedUser.uid;
      if (uid == 0L)
        return;
      Navigator.Current.NavigateToUserProfile(uid, "", "", false);
    }

    protected override void GenerateChildren()
    {
      base.GenerateChildren();
      TextBlock textBlock1 = new TextBlock()
      {
        Foreground = (Brush) (Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush),
        FontFamily = new FontFamily("Segoe WP Semibold")
      };
      textBlock1.Margin = new Thickness(50.0, -8.0, 0.0, 0.0);
      textBlock1.Text = this._mvm.UIUserName ?? "";
      this.Children.Add((FrameworkElement) textBlock1);
      TextBlock textBlock2 = new TextBlock()
      {
        Foreground = (Brush) (Application.Current.Resources["PhoneVKSubtleBrush"] as SolidColorBrush)
      };
      textBlock2.Margin = new Thickness(50.0, 18.0, 0.0, 0.0);
      textBlock2.Text = this._mvm.UIDate ?? "";
      this.Children.Add((FrameworkElement) textBlock2);
    }
  }
}
