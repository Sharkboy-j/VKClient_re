using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
    public class DateTimeCustomPickerUC : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DateTimeCustomPickerUC), new PropertyMetadata(new PropertyChangedCallback(DateTimeCustomPickerUC.OnTitleChanged)));
        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(DateTimeCustomPickerUC), new PropertyMetadata(new PropertyChangedCallback(DateTimeCustomPickerUC.OnContentTextChanged)));
        internal TextBlock textBlockTitle;
        internal Button buttonContent;
        private bool _contentLoaded;

        public string Title
        {
            get
            {
                return (string)this.GetValue(DateTimeCustomPickerUC.TitleProperty);
            }
            set
            {
                this.SetValue(DateTimeCustomPickerUC.TitleProperty, (object)value);
            }
        }

        public string ContentText
        {
            get
            {
                return (string)this.GetValue(DateTimeCustomPickerUC.ContentTextProperty);
            }
            set
            {
                this.SetValue(DateTimeCustomPickerUC.ContentTextProperty, (object)value);
            }
        }

        public event RoutedEventHandler Click;

        public DateTimeCustomPickerUC()
        {
            this.InitializeComponent();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimeCustomPickerUC timeCustomPickerUc = d as DateTimeCustomPickerUC;
            if (timeCustomPickerUc == null)
                return;
            string str = e.NewValue as string;
            if (string.IsNullOrEmpty(str))
                return;
            timeCustomPickerUc.textBlockTitle.Text = str;
        }

        private static void OnContentTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DateTimeCustomPickerUC timeCustomPickerUc = d as DateTimeCustomPickerUC;
            if (timeCustomPickerUc == null)
                return;
            string str = e.NewValue as string;
            if (string.IsNullOrEmpty(str))
                return;
            timeCustomPickerUc.buttonContent.Content = (object)str;
        }

        private void ButtonContent_OnClicked(object sender, RoutedEventArgs e)
        {
            if (this.Click == null)
                return;
            this.Click((object)this, e);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/DateTimeCustomPickerUC.xaml", UriKind.Relative));
            this.textBlockTitle = (TextBlock)this.FindName("textBlockTitle");
            this.buttonContent = (Button)this.FindName("buttonContent");
        }
    }
}
