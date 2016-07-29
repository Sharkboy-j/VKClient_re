using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
    public partial class PrivacyHeaderUC : UserControl
    {
        public Action OnTap { get; set; }

        public PrivacyHeaderUC()
        {
            this.InitializeComponent();
        }

        private void LayoutRoot_Tap(object sender, GestureEventArgs e)
        {
            //if (base.OnTap == null)
            //return;
            base.OnTap(e);
        }
    }
}
