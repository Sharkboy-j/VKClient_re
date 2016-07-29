using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Framework;
using VKMessenger.Library.Events;

namespace VKMessenger.Library.VirtItems
{
    public class TapHandlerItem : VirtualizableItemBase
    {
        private Grid _grid;
        private MessageViewModel _mvm;

        public override double FixedHeight
        {
            get
            {
                return 10.0;
            }
        }

        public TapHandlerItem(double width, double height, MessageViewModel mvm)
            : base(width)
        {
            this._mvm = mvm;
            this._grid = new Grid();
            this.SetWidthHeight(width, height);
            this._grid.Background = (Brush)new SolidColorBrush(Colors.Transparent);
        }

        public void SetWidthHeight(double width, double height)
        {
            this._grid.Width = width;
            this._grid.Height = height;
        }

        protected override void GenerateChildren()
        {
            base.GenerateChildren();
            this._grid.IsHitTestVisible = this._mvm.IsInSelectionMode;
            this._grid.Tap += new EventHandler<GestureEventArgs>(this._grid_Tap);
            this._mvm.PropertyChanged += new PropertyChangedEventHandler(this._mvm_PropertyChanged);
            this.Children.Add((FrameworkElement)this._grid);
        }

        private void _grid_Tap(object sender, GestureEventArgs e)
        {
            EventAggregator.Current.Publish((object)new MessageActionEvent()
            {
                Message = this._mvm,
                MessageActionType = MessageActionType.SelectUnselect
            });
        }

        protected override void ReleaseResourcesOnUnload()
        {
            base.ReleaseResourcesOnUnload();
            this._grid.Tap -= new EventHandler<GestureEventArgs>(this._grid_Tap);
            this._mvm.PropertyChanged -= new PropertyChangedEventHandler(this._mvm_PropertyChanged);
        }

        private void _mvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsInSelectionMode"))
                return;
            this._grid.IsHitTestVisible = this._mvm.IsInSelectionMode;
        }
    }
}
