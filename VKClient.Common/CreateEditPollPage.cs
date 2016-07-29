using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Emoji;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class CreateEditPollPage : PageBase
  {
    private ApplicationBarIconButton _appBarButtonCheck = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private ApplicationBarIconButton _appBarButtonCancel = new ApplicationBarIconButton()
    {
      IconUri = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private ApplicationBar _mainAppBar = new ApplicationBar()
    {
      BackgroundColor = VKConstants.AppBarBGColor,
      ForegroundColor = VKConstants.AppBarFGColor,
      Opacity = 0.9
    };
    private readonly DelayedExecutor _de = new DelayedExecutor(100);
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal ScrollViewer scrollViewer;
    internal StackPanel stackPanel;
    internal TextBox textBoxQuestion;
    internal InlineAddButtonUC ucAddOption;
    internal TextBoxPanelControl textBoxPanel;
    private bool _contentLoaded;

    private CreateEditPollViewModel VM
    {
      get
      {
        return this.DataContext as CreateEditPollViewModel;
      }
    }

    public CreateEditPollPage()
    {
      this.InitializeComponent();
      this.ucAddOption.Text = CommonResources.Poll_AddAnOption;
      this.ucAddOption.OnAdd = new Action(this.AddOption);
      this.Loaded += new RoutedEventHandler(this.CreateEditPollPage_Loaded);
    }

    private void BuildAppBar()
    {
      this._appBarButtonCheck.Click += new EventHandler(this._appBarButtonCheck_Click);
      this._appBarButtonCancel.Click += new EventHandler(this._appBarButtonCancel_Click);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonCheck);
      this._mainAppBar.Buttons.Add((object) this._appBarButtonCancel);
      this.ApplicationBar = (IApplicationBar) this._mainAppBar;
    }

    private void UpdateAppBar()
    {
      this._appBarButtonCheck.IsEnabled = this.VM.CanSave;
    }

    private void _appBarButtonCancel_Click(object sender, EventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void _appBarButtonCheck_Click(object sender, EventArgs e)
    {
      this.VM.SavePoll((Action<Poll>) (poll =>
      {
        ParametersRepository.SetParameterForId("UpdatedPoll", (object) poll);
        Navigator.Current.GoBack();
      }));
    }

    private void CreateEditPollPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.VM != null && this.VM.CurrentMode == CreateEditPollViewModel.Mode.Create)
        this.textBoxQuestion.Focus();
      this.Loaded -= new RoutedEventHandler(this.CreateEditPollPage_Loaded);
    }

    private void AddOption()
    {
      this.VM.AddPollOption();
      this._de.AddToDelayedExecution((Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
      {
        List<TextBox> source = FramePageUtils.AllTextBoxes((DependencyObject) this.LayoutRoot);
        if (!source.Any<TextBox>())
          return;
        source.Last<TextBox>((Func<TextBox, bool>) (t =>
        {
          if (t.Tag != null)
            return t.Tag.ToString() == "RemovableTextBox";
          return false;
        })).Focus();
      }))));
      Deployment.Current.Dispatcher.BeginInvoke((Action) (() => {}));
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      this.textBoxQuestion.GetBindingExpression(TextBox.TextProperty).UpdateSource();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        long ownerId = long.Parse(this.NavigationContext.QueryString["OwnerId"]);
        long pollId = long.Parse(this.NavigationContext.QueryString["PollId"]);
        Poll poll = ParametersRepository.GetParameterForIdAndReset("Poll") as Poll;
        if (pollId != 0L)
          this.DataContext = (object) CreateEditPollViewModel.CreateForEditPoll(ownerId, pollId, poll);
        else
          this.DataContext = (object) CreateEditPollViewModel.CreateForNewPoll(ownerId);
        this.VM.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        this.BuildAppBar();
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "CanSave"))
        return;
      this.UpdateAppBar();
    }

    private void textBox_KeyUp(object sender, KeyEventArgs e)
    {
      TextBox textbox = sender as TextBox;
      if (textbox == null || string.IsNullOrWhiteSpace(textbox.Text) || e.Key != Key.Enter)
        return;
      TextBox nextTextBox = FramePageUtils.FindNextTextBox((DependencyObject) this.LayoutRoot, textbox);
      if (nextTextBox == null)
        return;
      nextTextBox.Focus();
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = true;
      FrameworkElement element = (FrameworkElement) sender;
      if (element.Name == this.textBoxQuestion.Name)
      {
        this.scrollViewer.ScrollToVerticalOffset(0.0);
        this.UpdateLayout();
      }
      StackPanel stackPanel = this.stackPanel;
      this.scrollViewer.ScrollToOffsetWithAnimation(element.GetRelativePosition((UIElement) stackPanel).Y - 38.0, 0.2, false);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.textBoxPanel.IsOpen = false;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/CreateEditPollPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.scrollViewer = (ScrollViewer) this.FindName("scrollViewer");
      this.stackPanel = (StackPanel) this.FindName("stackPanel");
      this.textBoxQuestion = (TextBox) this.FindName("textBoxQuestion");
      this.ucAddOption = (InlineAddButtonUC) this.FindName("ucAddOption");
      this.textBoxPanel = (TextBoxPanelControl) this.FindName("textBoxPanel");
    }
  }
}
