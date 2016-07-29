using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VKClient.Groups.UC
{
    public partial class CommunityDomainTextBox : UserControl
  {
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string), typeof (CommunityDomainTextBox), new PropertyMetadata((object) "", new PropertyChangedCallback(CommunityDomainTextBox.TextPropertyChangedCallback)));
   

    public string Text
    {
      get
      {
        return (string) this.GetValue(CommunityDomainTextBox.TextProperty);
      }
      set
      {
        this.SetValue(CommunityDomainTextBox.TextProperty, (object) value);
      }
    }

    public event TextChangedEventHandler TextChanged;

    public new event RoutedEventHandler GotFocus;

    public new event RoutedEventHandler LostFocus;

    public CommunityDomainTextBox()
    {
      this.InitializeComponent();
    }

    private void OnClicked(object sender, GestureEventArgs e)
    {
      this.ContentBox.Focus();
    }

    private void ContentBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.FocusOwner.Focus();
    }

    private void ContentBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.BackgroundBorder.BorderBrush = (Brush) Application.Current.Resources["PhoneTextBoxDefaultFocusedBorderBrush"];
      RoutedEventHandler routedEventHandler = this.GotFocus;
      if (routedEventHandler == null)
        return;
      object sender1 = sender;
      RoutedEventArgs e1 = e;
      routedEventHandler(sender1, e1);
    }

    private void ContentBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.BackgroundBorder.BorderBrush = (Brush) Application.Current.Resources["PhoneTextBoxDefaultBorderBrush"];
      RoutedEventHandler routedEventHandler = this.LostFocus;
      if (routedEventHandler == null)
        return;
      object sender1 = sender;
      RoutedEventArgs e1 = e;
      routedEventHandler(sender1, e1);
    }

    private void ContentBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.Text = this.ContentBox.Text;
      TextChangedEventHandler changedEventHandler = this.TextChanged;
      if (changedEventHandler == null)
        return;
      TextChangedEventArgs e1 = e;
      changedEventHandler((object) this, e1);
    }

    private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      TextBox textBox = ((CommunityDomainTextBox) sender).ContentBox;
      string str = (string) e.NewValue;
      if (!(textBox.Text != str))
        return;
      textBox.Text = str;
    }
  }
}
