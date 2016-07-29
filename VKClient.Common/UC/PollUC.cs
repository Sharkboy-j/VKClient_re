using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class PollUC : UserControlVirtualizable
  {
    private static readonly PollAnswerUC _pollAnswerUC = new PollAnswerUC();
    private const double POLL_ANSWER_WIDTH = 448.0;
    private PollViewModel _viewModel;
    internal ControlTemplate MenuItemTemplate;
    internal Grid gridBackground;
    internal TextBlock textBlockTitle;
    internal TextBlock textBlockDescrption;
    private bool _contentLoaded;

    public PollUC()
    {
      this.InitializeComponent();
    }

    public void Initialize(Poll poll, long topicId = 0)
    {
      this._viewModel = new PollViewModel(poll, topicId);
      this.DataContext = (object) this._viewModel;
    }

    public double CalculateTotalHeight()
    {
      return this.CalculateTotalHeight1();
    }

    private double CalculateTotalHeight1()
    {
      double num1 = 0.0;
      Thickness margin1 = this.textBlockTitle.Margin;
      double num2 = margin1.Top + this.textBlockTitle.ActualHeight;
      margin1 = this.textBlockTitle.Margin;
      double bottom1 = margin1.Bottom;
      double num3 = num2 + bottom1;
      double num4 = num1 + num3;
      Thickness margin2 = this.textBlockDescrption.Margin;
      double num5 = margin2.Top + this.textBlockDescrption.ActualHeight;
      margin2 = this.textBlockDescrption.Margin;
      double bottom2 = margin2.Bottom;
      double num6 = num5 + bottom2;
      double num7 = num4 + num6;
      foreach (PollAnswerHeader answer in this._viewModel.Answers)
      {
        PollUC._pollAnswerUC.DataContext = (object) answer;
        PollUC._pollAnswerUC.Measure(new Size(448.0, double.PositiveInfinity));
        num7 += PollUC._pollAnswerUC.DesiredSize.Height;
        num7 += 8.0;
      }
      return num7 + 8.0;
    }

    private double CalculateTotalHeight2()
    {
      double num1 = 0.0;
      Thickness margin1 = this.textBlockTitle.Margin;
      double num2 = margin1.Top + this.textBlockTitle.ActualHeight;
      margin1 = this.textBlockTitle.Margin;
      double bottom1 = margin1.Bottom;
      double num3 = num2 + bottom1;
      double num4 = num1 + num3;
      Thickness margin2 = this.textBlockDescrption.Margin;
      double num5 = margin2.Top + this.textBlockDescrption.ActualHeight;
      margin2 = this.textBlockDescrption.Margin;
      double bottom2 = margin2.Bottom;
      double num6 = num5 + bottom2;
      double num7 = num4 + num6;
      foreach (PollAnswerHeader answer in this._viewModel.Answers)
      {
        num7 += TextBlockMeasurementHelper.MeasureHeight(332.0, answer.AnswerStr, new FontFamily("Segoe WP"), 20.0, 26.0, LineStackingStrategy.BlockLineHeight, TextWrapping.Wrap, new Thickness(12.0, 8.0, 96.0, 14.0));
        num7 += 8.0;
      }
      return num7 + 8.0;
    }

    private void PollOption_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      if (this._viewModel.IsVoting)
        return;
      FrameworkElement frameworkElement = sender as FrameworkElement;
      PollAnswerHeader pollAnswerHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as PollAnswerHeader;
      if (pollAnswerHeader == null)
        return;
      if (!this._viewModel.Voted)
      {
        this._viewModel.VoteUnvote(pollAnswerHeader);
      }
      else
      {
        DependencyObject element = sender as DependencyObject;
        List<MenuItemExtended> menuItemExtendedList1 = new List<MenuItemExtended>();
        if (this._viewModel.Poll.answer_id == pollAnswerHeader.Answer.id)
        {
          List<MenuItemExtended> menuItemExtendedList2 = menuItemExtendedList1;
          MenuItemExtended menuItemExtended = new MenuItemExtended();
          menuItemExtended.Id = 1;
          menuItemExtended.Title = CommonResources.Poll_CancelChoice;
          Action action = (Action) (() => this._viewModel.VoteUnvote(pollAnswerHeader));
          menuItemExtended.Action = action;
          menuItemExtendedList2.Add(menuItemExtended);
        }
        if (this._viewModel.Poll.anonymous == 0 && pollAnswerHeader.Answer.votes > 0)
        {
          string str = UIStringFormatterHelper.FormatNumberOfSomething(pollAnswerHeader.Answer.votes, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
          List<MenuItemExtended> menuItemExtendedList2 = menuItemExtendedList1;
          MenuItemExtended menuItemExtended = new MenuItemExtended();
          menuItemExtended.Id = 2;
          menuItemExtended.Title = CommonResources.Poll_VotedList;
          menuItemExtended.Description = str;
          Action action = (Action) (() => Navigator.Current.NavigateToPollVoters(this._viewModel.Poll.owner_id, this._viewModel.Poll.id, pollAnswerHeader.Answer.id, pollAnswerHeader.Answer.text));
          menuItemExtended.Action = action;
          menuItemExtendedList2.Add(menuItemExtended);
        }
        if (menuItemExtendedList1.Count == 0)
          return;
        ContextMenu contextMenu1 = new ContextMenu();
        contextMenu1.IsZoomEnabled = false;
        SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
        contextMenu1.Background = (Brush) solidColorBrush1;
        SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
        contextMenu1.Foreground = (Brush) solidColorBrush2;
        ContextMenu contextMenu2 = contextMenu1;
        foreach (MenuItemExtended menuItemExtended1 in menuItemExtendedList1)
        {
          MenuItem menuItem1 = new MenuItem();
          ControlTemplate controlTemplate = (ControlTemplate) this.Resources["MenuItemTemplate"];
          menuItem1.Template = controlTemplate;
          MenuItemExtended menuItemExtended2 = menuItemExtended1;
          menuItem1.DataContext = (object) menuItemExtended2;
          Thickness thickness = menuItemExtended1.Id == 2 ? new Thickness(0.0, 0.0, 0.0, 0.0) : new Thickness(0.0, 8.0, 0.0, 19.0);
          menuItem1.Margin = thickness;
          MenuItem menuItem2 = menuItem1;
          menuItem2.Click += new RoutedEventHandler(this.PollOptionMenuItem_OnClick);
          contextMenu2.Items.Add((object) menuItem2);
        }
        ContextMenuService.SetContextMenu(element, contextMenu2);
        contextMenu2.IsOpen = true;
      }
    }

    private void PollOptionMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      if (frameworkElement == null)
        return;
      MenuItemExtended menuItemExtended = frameworkElement.DataContext as MenuItemExtended;
      if (menuItemExtended == null || menuItemExtended.Action == null)
        return;
      menuItemExtended.Action();
    }

    private void GridContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      double height = e.NewSize.Height;
      if (double.IsInfinity(height) || double.IsNaN(height))
        return;
      this.gridBackground.Height = height;
      this.gridBackground.Children.Clear();
      Rectangle rectangle = new Rectangle();
      double num1 = 1.0;
      rectangle.Width = num1;
      double num2 = height;
      rectangle.Height = num2;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneAttachmentBorderBrush"];
      rectangle.Fill = (Brush) solidColorBrush1;
      int num3 = 0;
      rectangle.VerticalAlignment = (VerticalAlignment) num3;
      int num4 = 0;
      rectangle.HorizontalAlignment = (HorizontalAlignment) num4;
      Rectangle rect1 = rectangle;
      Rectangle rect2 = new Rectangle();
      double num5 = 1.0;
      rect2.Width = num5;
      double num6 = height;
      rect2.Height = num6;
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneAttachmentBorderBrush"];
      rect2.Fill = (Brush) solidColorBrush2;
      int num7 = 0;
      rect2.VerticalAlignment = (VerticalAlignment) num7;
      int num8 = 2;
      rect2.HorizontalAlignment = (HorizontalAlignment) num8;
      List<Rectangle> rectangleList1 = RectangleHelper.CoverByRectangles(rect1);
      List<Rectangle> rectangleList2 = RectangleHelper.CoverByRectangles(rect2);
      foreach (UIElement uiElement in rectangleList1)
        this.gridBackground.Children.Add(uiElement);
      foreach (UIElement uiElement in rectangleList2)
        this.gridBackground.Children.Add(uiElement);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/UC/PollUC.xaml", UriKind.Relative));
      this.MenuItemTemplate = (ControlTemplate) this.FindName("MenuItemTemplate");
      this.gridBackground = (Grid) this.FindName("gridBackground");
      this.textBlockTitle = (TextBlock) this.FindName("textBlockTitle");
      this.textBlockDescrption = (TextBlock) this.FindName("textBlockDescrption");
    }
  }
}
