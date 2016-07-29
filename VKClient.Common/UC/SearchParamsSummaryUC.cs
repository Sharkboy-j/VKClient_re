using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace VKClient.Common.UC
{
    public class SearchParamsSummaryUC : UserControl
    {
        public static readonly DependencyProperty IsSeparatorVisibleProperty = DependencyProperty.Register("IsSeparatorVisible", typeof(bool), typeof(SearchParamsSummaryUC), new PropertyMetadata((object)true, new PropertyChangedCallback(SearchParamsSummaryUC.IsSeparatorVisible_OnChanged)));
        internal Rectangle rectSeparator;
        private bool _contentLoaded;

        public bool IsSeparatorVisible
        {
            get
            {
                return (bool)this.GetValue(SearchParamsSummaryUC.IsSeparatorVisibleProperty);
            }
            set
            {
                this.SetValue(SearchParamsSummaryUC.IsSeparatorVisibleProperty, (object)value);
            }
        }

        private SearchParamsViewModel VM
        {
            get
            {
                return this.DataContext as SearchParamsViewModel;
            }
        }

        public event EventHandler ClearButtonTap;

        public SearchParamsSummaryUC()
        {
            this.InitializeComponent();
        }

        private static void IsSeparatorVisible_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchParamsSummaryUC)d).rectSeparator.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OpenParamsPage(object sender, GestureEventArgs e)
        {
            SearchParamsViewModel vm = this.VM;
            if (vm == null)
                return;
            vm.NavigateToParametersPage();
        }

        private void Clear_OnTap(object sender, GestureEventArgs e)
        {
            SearchParamsViewModel vm = this.VM;
            if (vm != null)
                vm.Clear();
            if (this.ClearButtonTap == null)
                return;
            this.ClearButtonTap((object)this, EventArgs.Empty);
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/SearchParamsSummaryUC.xaml", UriKind.Relative));
            this.rectSeparator = (Rectangle)this.FindName("rectSeparator");
        }
    }
}
