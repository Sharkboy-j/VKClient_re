using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class RepostHeaderUC : UserControlVirtualizable
  {
    public const double FIXED_HEIGHT = 56.0;
    private Action _callbackTap;
    internal Grid gridRoot;
    internal Image imageUserOrGroup;
    internal TextBlock textBlockUserOrGroupName;
    internal TextBlock textBlockDate;
    internal Border postSourceBorder;
    private bool _contentLoaded;

    public RepostHeaderUC()
    {
      this.InitializeComponent();
    }

    public void Configure(WallRepostInfo configuration, Action callbackTap)
    {
      if (configuration != null)
      {
        ImageLoader.SetUriSource(this.imageUserOrGroup, configuration.Pic);
        this.textBlockUserOrGroupName.Text = configuration.Name;
        this.textBlockDate.Text = configuration.Subtitle;
        this.textBlockUserOrGroupName.CorrectText(configuration.Width - this.textBlockUserOrGroupName.Margin.Left);
        string iconUri = configuration.PostSourcePlatform.GetIconUri();
        if (!string.IsNullOrEmpty(iconUri))
        {
          this.postSourceBorder.Visibility = Visibility.Visible;
          ImageBrush imageBrush = new ImageBrush();
          ImageLoader.SetImageBrushMultiResSource(imageBrush, iconUri);
          this.postSourceBorder.OpacityMask = (Brush) imageBrush;
        }
        else
          this.postSourceBorder.Visibility = Visibility.Collapsed;
      }
      this._callbackTap = callbackTap;
      if (this._callbackTap == null)
        return;
      MetroInMotion.SetTilt((DependencyObject) this.gridRoot, 2.1);
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
      if (this._callbackTap == null)
        return;
      this._callbackTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/RepostHeaderUC.xaml", UriKind.Relative));
      this.gridRoot = (Grid) this.FindName("gridRoot");
      this.imageUserOrGroup = (Image) this.FindName("imageUserOrGroup");
      this.textBlockUserOrGroupName = (TextBlock) this.FindName("textBlockUserOrGroupName");
      this.textBlockDate = (TextBlock) this.FindName("textBlockDate");
      this.postSourceBorder = (Border) this.FindName("postSourceBorder");
    }
  }
}
