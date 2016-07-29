using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;
using VKClient.Common.Framework.SharpDXExt;
using VKClient.Common.Library.Posts;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC.InplaceGifViewer
{
  public class InplaceGifViewerUC : UserControlVirtualizable, INotifiableWhenOnScreenCenter, IHandleTap
  {
    private string _currentlyAssignedFile = "";
    private static int _mpCount;
    private static Grid _gridWithAttachedPlayer;
    internal Grid LayoutRoot;
    internal GifOverlayUC gifOverlayUC;
    private bool _contentLoaded;

    public InplaceGifViewerViewModel VM
    {
      get
      {
        return this.DataContext as InplaceGifViewerViewModel;
      }
      set
      {
        if (this.VM != null)
          this.VM.PropertyChanged -= new PropertyChangedEventHandler(this.VM_PropertyChanged);
        this.DataContext = (object) value;
        if (this.VM == null)
          return;
        this.VM.PropertyChanged += new PropertyChangedEventHandler(this.VM_PropertyChanged);
        this.UpdateVideoPlayer(false);
      }
    }

    public InplaceGifViewerUC()
    {
      this.InitializeComponent();
      this.SizeChanged += new SizeChangedEventHandler(this.InplaceGifViewerUC_SizeChanged);
    }

    private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "LocalVideoFile"))
        return;
      this.UpdateVideoPlayer(false);
    }

    private void UpdateVideoPlayer(bool force = false)
    {
      string file = this.VM.LocalVideoFile;
      if (this._currentlyAssignedFile == file && !force)
        return;
      if (InplaceGifViewerUC._gridWithAttachedPlayer != null)
      {
        InplaceGifViewerUC._gridWithAttachedPlayer.Children.RemoveAt(0);
        InplaceGifViewerUC._gridWithAttachedPlayer = (Grid) null;
        --InplaceGifViewerUC._mpCount;
      }
      this._currentlyAssignedFile = file;
      if (string.IsNullOrWhiteSpace(file))
        return;
      if (this.VM.UseOldGifPlayer)
      {
        this.Dispatcher.BeginInvoke((Action) (() =>
        {
          GifViewerUC gifViewerUc = new GifViewerUC();
          gifViewerUc.Init(file, this.VM.DocHeader.GetSize());
          this.LayoutRoot.Children.Insert(0, (UIElement) gifViewerUc);
          InplaceGifViewerUC._gridWithAttachedPlayer = this.LayoutRoot;
        }));
      }
      else
      {
        DxMediaElement dxMediaElement = new DxMediaElement();
        int num = 0;
        dxMediaElement.IsHitTestVisible = num != 0;
        DxMediaElement mediaElement = dxMediaElement;
        this.LayoutRoot.Children.Insert(0, (UIElement) mediaElement);
        InplaceGifViewerUC._gridWithAttachedPlayer = this.LayoutRoot;
        ++InplaceGifViewerUC._mpCount;
        this.Dispatcher.BeginInvoke((Action) (() => mediaElement.VideoPath = file));
      }
    }

    private void InplaceGifViewerUC_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.PreviousSize.Width <= 0.0 || e.PreviousSize.Height <= 0.0 || e.PreviousSize.Width == e.NewSize.Width && e.PreviousSize.Height == e.NewSize.Height)
        return;
      this.UpdateVideoPlayer(true);
    }

    public override void ReleaseResources()
    {
      base.ReleaseResources();
      this.VM.ReleaseResorces();
    }

    public void NotifyInTheCenterOfScreen()
    {
      this.VM.HandleOnScreenCenter();
    }

    public void OnTap()
    {
      this.VM.HandleTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/InplaceGifViewer/InplaceGifViewerUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.gifOverlayUC = (GifOverlayUC) this.FindName("gifOverlayUC");
    }
  }
}
