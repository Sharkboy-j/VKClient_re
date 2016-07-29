using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class NewsFeedHideActionsUC : UserControlVirtualizable
  {
    private NewsFeedHideActionsViewModel _viewModel;
    private Action<long> _hideSourceItemsCallback;
    private Action _cancelCallback;
    private bool _contentLoaded;

    public NewsFeedHideActionsUC()
    {
      this.InitializeComponent();
    }

    public void Initialize(NewsFeedHideActionsViewModel viewModel, Action<long> hideSourceItemsCallback, Action cancelCallback)
    {
      this._viewModel = viewModel;
      this._hideSourceItemsCallback = hideSourceItemsCallback;
      this._cancelCallback = cancelCallback;
    }

    private void Cancel_OnClick(object sender, GestureEventArgs args)
    {
      NewsFeedHideActionsViewModel actionsViewModel = this._viewModel;
      if (actionsViewModel != null)
        actionsViewModel.Cancel();
      Action action = this._cancelCallback;
      if (action == null)
        return;
      action();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/NewsFeedHideActionsUC.xaml", UriKind.Relative));
    }
  }
}
