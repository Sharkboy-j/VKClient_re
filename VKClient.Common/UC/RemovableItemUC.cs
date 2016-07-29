using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
    public class RemovableItemUC : UserControl
    {
        internal TextBox textBoxText;
        private bool _contentLoaded;

        public IRemovableWithText VM
        {
            get
            {
                return this.DataContext as IRemovableWithText;
            }
        }

        public new event RoutedEventHandler GotFocus;

        public new event RoutedEventHandler LostFocus;

        public RemovableItemUC()
        {
            this.InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.VM == null)
                return;
            this.VM.Text = this.textBoxText.Text;
        }

        private void RemoveOptionTap(object sender, GestureEventArgs e)
        {
            if (this.VM == null)
                return;
            this.VM.Remove();
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox == null || string.IsNullOrWhiteSpace(textbox.Text) || e.Key != Key.Enter)
                return;
            TextBox nextTextBox = FramePageUtils.FindNextTextBox((DependencyObject)FramePageUtils.CurrentPage, textbox);
            if (nextTextBox == null || nextTextBox.Tag == null || !(nextTextBox.Tag.ToString() == "RemovableTextBox"))
                return;
            nextTextBox.Focus();
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (this.GotFocus == null)
                return;
            this.GotFocus(sender, e);
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (this.LostFocus == null)
                return;
            this.LostFocus(sender, e);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/RemovableItemUC.xaml", UriKind.Relative));
            this.textBoxText = (TextBox)this.FindName("textBoxText");
        }
    }
}
