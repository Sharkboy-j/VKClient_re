using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VKClient.Common.Framework;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class NewsfeedTopToggleUC : UserControl, IHandle<NewsfeedTopEnabledDisabledEvent>, IHandle
  {
    internal Border borderFadeOut;
    private bool _contentLoaded;

    public event EventHandler ToggleControlTap;

    public NewsfeedTopToggleUC()
    {
      this.InitializeComponent();
      EventAggregator.Current.Subscribe((object) this);
      this.borderFadeOut.Opacity = 0.0;
      this.Loaded += (RoutedEventHandler) ((sender, args) =>
      {
        PickableNewsfeedSourceItemViewModel sourceItemViewModel = this.DataContext as PickableNewsfeedSourceItemViewModel;
        if (sourceItemViewModel == null || !sourceItemViewModel.FadeOutToggleEnabled)
          return;
        this.borderFadeOut.Animate(1.0, 0.0, (object) UIElement.OpacityProperty, 2000, new int?(), null, null);
      });
    }

    private void ToggleTopNewsContainer_OnTap(object sender, GestureEventArgs e)
    {
      e.Handled = true;
    }

    public void Handle(NewsfeedTopEnabledDisabledEvent message)
    {
      EventHandler eventHandler = this.ToggleControlTap;
      if (eventHandler == null)
        return;
      EventArgs e = EventArgs.Empty;
      eventHandler((object) this, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewsfeedTopToggleUC.xaml", UriKind.Relative));
      this.borderFadeOut = (Border) this.FindName("borderFadeOut");
    }
  }
}
