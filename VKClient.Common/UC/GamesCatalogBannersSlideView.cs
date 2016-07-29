using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VKClient.Common.Emoji;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library.Games;
using Windows.Foundation;

namespace VKClient.Common.UC
{
    public class GamesCatalogBannersSlideView : UserControl, INotifyPropertyChanged
    {
        private DispatcherTimer _nextElementSwipeTimer = new DispatcherTimer();
        private bool _moveNext = true;
        private const double MOVE_TO_NEXT_VELOCITY_THRESHOLD = 100.0;
        private const double HIDE_VELOCITY_THRESHOLD = 100.0;
        private const int DURATION_BOUNCING = 175;
        private const int DURATION_MOVE_TO_NEXT = 200;
        private static readonly IEasingFunction ANIMATION_EASING;
        public static readonly DependencyProperty BackgroundColorProperty;
        public static readonly DependencyProperty IsCycledProperty;
        private ObservableCollection<object> _items;
        private int _selectedIndex;
        private List<Control> _elements;
        private bool _isReadyToHideFired;
        private bool _isAnimating;
        private bool _isInVerticalSwipe;
        internal Grid gridRoot;
        internal Grid LayoutRoot;
        private bool _contentLoaded;

        public Func<Control> CreateSingleElement { get; set; }

        public bool ChangeIndexBeforeAnimation { get; set; }

        public double NextElementMargin { get; set; }

        public bool IsScrollListeningEnabled { get; set; }

        public bool AllowVerticalSwipe { get; set; }

        public TimeSpan NextElementSwipeDelay { get; set; }

        public Brush BackgroundColor
        {
            get
            {
                return (Brush)this.GetValue(GamesCatalogBannersSlideView.BackgroundColorProperty);
            }
            set
            {
                this.SetValue(GamesCatalogBannersSlideView.BackgroundColorProperty, (object)value);
            }
        }

        public bool IsCycled
        {
            get
            {
                return (bool)this.GetValue(GamesCatalogBannersSlideView.IsCycledProperty);
            }
            set
            {
                this.SetValue(GamesCatalogBannersSlideView.IsCycledProperty, (object)value);
            }
        }

        public ObservableCollection<object> Items
        {
            get
            {
                return this._items;
            }
            set
            {
                if (this._items != null)
                    this._items.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
                this._items = value;
                if (this._items != null)
                    this._items.CollectionChanged += new NotifyCollectionChangedEventHandler(this.ItemsOnCollectionChanged);
                this.OnPropertyChanged("Items");
                this.EnsureElements();
                this.ArrangeElements();
                this.SelectedIndex = 0;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }
            set
            {
                int num = this._selectedIndex;
                this._selectedIndex = value;
                this.OnPropertyChanged("SelectedIndex");
                if (this._selectedIndex - num == 2)
                {
                    GamesCatalogBannersSlideView.Swap(this._elements, 0, 2);
                    this.UpdateSources(false, true, true);
                    this.ArrangeElements();
                }
                else if (num - this._selectedIndex == 2)
                {
                    GamesCatalogBannersSlideView.Swap(this._elements, 0, 2);
                    this.UpdateSources(true, true, false);
                    this.ArrangeElements();
                }
                else if (this._selectedIndex - num == 1)
                {
                    this.MoveToNextOrPrevious(true);
                    this.ArrangeElements();
                }
                else if (num - this._selectedIndex == 1)
                {
                    this.MoveToNextOrPrevious(false);
                    this.ArrangeElements();
                }
                else
                    this.UpdateSources(false, new bool?());
            }
        }

        private Control CurrentElement
        {
            get
            {
                return this._elements[1];
            }
        }

        private double ArrangeWidth
        {
            get
            {
                return this.ActualWidth - this.NextElementMargin;
            }
        }

        public event TypedEventHandler<GamesCatalogBannersSlideView, int> SelectionChanged;// UPDATE: 4.8.0
        //{
        //add
        //{
        //}
        //remove
        //{
        //}
        //}

        public event EventHandler ItemsCleared;

        //public event EventHandler SwipedToHide;

        public event PropertyChangedEventHandler PropertyChanged;

        static GamesCatalogBannersSlideView()
        {
            CubicEase cubicEase = new CubicEase();
            int num = 0;
            cubicEase.EasingMode = (EasingMode)num;
            GamesCatalogBannersSlideView.ANIMATION_EASING = (IEasingFunction)cubicEase;
            GamesCatalogBannersSlideView.BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Brush), typeof(GamesCatalogBannersSlideView), new PropertyMetadata(new PropertyChangedCallback(GamesCatalogBannersSlideView.OnBackgroundBrushChanged)));
            GamesCatalogBannersSlideView.IsCycledProperty = DependencyProperty.Register("IsCycled", typeof(bool), typeof(GamesCatalogBannersSlideView), new PropertyMetadata((object)false));
        }

        public GamesCatalogBannersSlideView()
        {
            this.InitializeComponent();
            this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
            this.Loaded += new RoutedEventHandler(this.OnLoaded);
            this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
            this.DataContext = (object)this;
        }

        public GamesCatalogBannersSlideView(double nextElementMargin)
            : this()
        {
            this.NextElementMargin = nextElementMargin;
        }

        private static void OnBackgroundBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GamesCatalogBannersSlideView bannersSlideView = d as GamesCatalogBannersSlideView;
            if (bannersSlideView == null)
                return;
            Brush brush = e.NewValue as Brush;
            if (brush == null)
                return;
            bannersSlideView.gridRoot.Background = brush;
        }

        private void NextElementSwipeTimer_OnTick(object sender, EventArgs eventArgs)
        {
            if (this.Items.Count < 2)
                return;
            this._isAnimating = true;
            if (this.IsCycled)
            {
                if (this.SelectedIndex == 0)
                    this._moveNext = true;
                else if (this.SelectedIndex == this.Items.Count - 1)
                    this._moveNext = false;
            }
            UIElement element1;
            UIElement element2;
            if (this._moveNext)
            {
                element1 = (UIElement)this._elements[1];
                element2 = (UIElement)this._elements[2];
            }
            else
            {
                element1 = (UIElement)this._elements[0];
                element2 = (UIElement)this._elements[1];
            }
            this.Move(element1, element2, this._moveNext, (Action)(() =>
            {
                this.MoveToNextOrPrevious(this._moveNext);
                this.ArrangeElements();
                this._isAnimating = false;
            }));
            this.ChangeCurrentInd(this._moveNext);
        }

        private void Move(UIElement element1, UIElement element2, bool next, Action completedCallback)
        {
            double num1 = -this.ArrangeWidth;
            if (!next)
                num1 *= -1.0;
            CubicEase cubicEase = new CubicEase();
            int num2 = 2;
            cubicEase.EasingMode = (EasingMode)num2;
            IEasingFunction easingFunction = (IEasingFunction)cubicEase;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            TranslateTransform translateTransform1 = element1.RenderTransform as TranslateTransform;
            TranslateTransform translateTransform2 = element2.RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform1.X,
                to = translateTransform1.X + num1,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = 500,
                target = (DependencyObject)translateTransform1,
                easing = easingFunction
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform2.X,
                to = translateTransform2.X + num1,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = 500,
                target = (DependencyObject)translateTransform2,
                easing = easingFunction
            });
            int? startTime = new int?(0);
            Action completed = completedCallback;
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this.NextElementSwipeDelay.Ticks <= 0L)
                return;
            this._nextElementSwipeTimer.Stop();
            this._nextElementSwipeTimer.Tick -= new EventHandler(this.NextElementSwipeTimer_OnTick);
            this._nextElementSwipeTimer = new DispatcherTimer()
            {
                Interval = this.NextElementSwipeDelay
            };
            this._nextElementSwipeTimer.Tick += new EventHandler(this.NextElementSwipeTimer_OnTick);
            this._nextElementSwipeTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (this._nextElementSwipeTimer == null)
                return;
            this._nextElementSwipeTimer.Stop();
            this._nextElementSwipeTimer.Tick -= new EventHandler(this.NextElementSwipeTimer_OnTick);
        }

        private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.UpdateSources(true, true, true);
            if (this.SelectedIndex >= this.Items.Count)
                this.SelectedIndex = this.Items.Count - 1;
            if (this.Items.Count != 0 || this.ItemsCleared == null)
                return;
            this.ItemsCleared((object)this, EventArgs.Empty);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.ArrangeElements();
        }

        private void ArrangeElements()
        {
            double num = this.SelectedIndex != 0 ? (this.SelectedIndex != this.Items.Count - 1 ? 0.0 : this.NextElementMargin) : 0.0;
            (this._elements[0].RenderTransform as TranslateTransform).X = -this.ArrangeWidth + num;
            (this._elements[1].RenderTransform as TranslateTransform).X = num;
            (this._elements[2].RenderTransform as TranslateTransform).X = this.ArrangeWidth + num;
        }

        private void UpdateSources(bool update0, bool update1, bool update2)
        {
            if (update1)
                this.SetDataContext((FrameworkElement)this._elements[1], this.GetItem(this._selectedIndex));
            if (update0)
                this.SetDataContext((FrameworkElement)this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (update2)
                this.SetDataContext((FrameworkElement)this._elements[2], this.GetItem(this._selectedIndex + 1));
            this.SetActiveState((FrameworkElement)this._elements[0], false);
            this.SetActiveState((FrameworkElement)this._elements[1], true);
            this.SetActiveState((FrameworkElement)this._elements[2], false);
        }

        private void UpdateSources(bool keepCurrentAsIs = false, bool? movedForvard = null)
        {
            if (!keepCurrentAsIs && !movedForvard.HasValue)
                this.SetDataContext((FrameworkElement)this._elements[1], this.GetItem(this._selectedIndex));
            int num = !movedForvard.HasValue ? 1 : (movedForvard.Value ? 1 : 0);
            if ((!movedForvard.HasValue ? 1 : (!movedForvard.Value ? 1 : 0)) != 0)
                this.SetDataContext((FrameworkElement)this._elements[0], this.GetItem(this._selectedIndex - 1));
            if (num != 0)
                this.SetDataContext((FrameworkElement)this._elements[2], this.GetItem(this._selectedIndex + 1));
            this.SetActiveState((FrameworkElement)this._elements[0], false);
            this.SetActiveState((FrameworkElement)this._elements[1], true);
            this.SetActiveState((FrameworkElement)this._elements[2], false);
        }

        private void SetDataContext(FrameworkElement frameworkElement, object dc)
        {
            ISupportDataContext supportDataContext = frameworkElement as ISupportDataContext;
            if (supportDataContext != null)
                supportDataContext.SetDataContext(dc);
            else
                frameworkElement.DataContext = dc;
        }

        public GameHeader GetCurrentGame()
        {
            CatalogBannerUC catalogBannerUc = this._elements[1] as CatalogBannerUC;
            if (catalogBannerUc != null)
                return catalogBannerUc.CatalogBanner;
            return (GameHeader)null;
        }

        private void SetActiveState(FrameworkElement frameworkElement, bool isActive)
        {
            ISupportState supportState = frameworkElement as ISupportState;
            if (supportState == null)
                return;
            supportState.SetState(isActive);
        }

        private object GetItem(int ind)
        {
            if (ind < 0 || ind >= this.Items.Count)
                return null;
            return this.Items[ind];
        }

        private void EnsureElements()
        {
            if (this._elements != null)
                return;
            this._elements = new List<Control>(3)
      {
        this.CreateSingleElement(),
        this.CreateSingleElement(),
        this.CreateSingleElement()
      };
            foreach (Control element in this._elements)
            {
                element.RenderTransform = (Transform)new TranslateTransform();
                element.CacheMode = (CacheMode)new BitmapCache();
                element.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationCompleted += new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                this.LayoutRoot.Children.Add((UIElement)element);
            }
        }

        private void Element_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            this._nextElementSwipeTimer.Stop();
        }

        private void Element_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.PinchManipulation != null)
                return;
            System.Windows.Point translation = e.DeltaManipulation.Translation;
            this.HandleDragDelta(translation.X, translation.Y);
        }

        private void Element_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            if (this.NextElementSwipeDelay.Ticks > 0L)
                this._nextElementSwipeTimer.Start();
            this.HandleDragCompleted(e.FinalVelocities.LinearVelocity.X);
        }

        private void HandleDragDelta(double hDelta, double vDelta)
        {
            if (this._isAnimating)
                return;
            TranslateTransform translateTransform = this.CurrentElement.RenderTransform as TranslateTransform;
            if ((translateTransform.X == 0.0 || Math.Abs(translateTransform.X) == this.NextElementMargin) && this.AllowVerticalSwipe && (hDelta == 0.0 && vDelta != 0.0 || Math.Abs(vDelta) / Math.Abs(hDelta) > 1.2))
            {
                if (vDelta < 0.0)
                    return;
                this._isInVerticalSwipe = true;
                foreach (UIElement element in this._elements)
                    (element.RenderTransform as TranslateTransform).Y += vDelta;
                if (translateTransform.Y <= 100.0 && vDelta <= 100.0 || this._isReadyToHideFired)
                    return;
                this._isReadyToHideFired = true;
                //if (this.SwipedToHide == null)
                //  return;
                //this.SwipedToHide((object) this, EventArgs.Empty);
            }
            else
            {
                if (this._isInVerticalSwipe)
                    return;
                if (this._selectedIndex == 0 && hDelta > 0.0 && translateTransform.X > 0.0 || this._selectedIndex == this._items.Count - 1 && hDelta < 0.0 && translateTransform.X < 0.0)
                    hDelta /= 3.0;
                foreach (UIElement element in this._elements)
                    (element.RenderTransform as TranslateTransform).X += hDelta;
            }
        }

        private void HandleDragCompleted(double hVelocity)
        {
            if (this._isAnimating)
                return;
            double num1 = hVelocity;
            bool? moveNext = new bool?();
            double x = (this.CurrentElement.RenderTransform as TranslateTransform).X;
            if ((this.CurrentElement.RenderTransform as TranslateTransform).Y < 100.0)
            {
                foreach (FrameworkElement element in this._elements)
                    this.AnimateElementVerticalOnDragComplete(element, 0.0);
            }
            this._isInVerticalSwipe = false;
            double num2 = num1;
            if ((num2 < -100.0 && x < 0.0 || x <= -this.Width / 2.0) && this._selectedIndex < this._items.Count - 1)
                moveNext = new bool?(true);
            else if ((num2 > 100.0 && x > 0.0 || x >= this.Width / 2.0) && this._selectedIndex > 0)
                moveNext = new bool?(false);
            bool flag1 = this.SelectedIndex <= 1;
            bool flag2 = this.SelectedIndex >= this.Items.Count - 2;
            bool? nullable1 = moveNext;
            bool flag3 = true;
            double num3;
            if ((nullable1.GetValueOrDefault() == flag3 ? (nullable1.HasValue ? 1 : 0) : 0) != 0)
            {
                num3 = !flag2 ? -this.ArrangeWidth : -this.ArrangeWidth + this.NextElementMargin;
            }
            else
            {
                bool? nullable2 = moveNext;
                bool flag4 = false;
                num3 = (nullable2.GetValueOrDefault() == flag4 ? (nullable2.HasValue ? 1 : 0) : 0) == 0 ? (this.SelectedIndex <= this.Items.Count - 2 ? (this.SelectedIndex >= 1 ? 0.0 : 0.0) : (this.Items.Count <= 1 ? 0.0 : this.NextElementMargin)) : (!flag1 ? this.ArrangeWidth : this.ArrangeWidth);
            }
            double delta = num3 - x;
            if (moveNext.HasValue && moveNext.Value)
            {
                this._isAnimating = true;
                this.AnimateTwoElementsOnDragComplete((FrameworkElement)this._elements[1], (FrameworkElement)this._elements[2], delta, (Action)(() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    this._isAnimating = false;
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else if (moveNext.HasValue && !moveNext.Value)
            {
                this._isAnimating = true;
                this.AnimateTwoElementsOnDragComplete((FrameworkElement)this._elements[0], (FrameworkElement)this._elements[1], delta, (Action)(() =>
                {
                    this.MoveToNextOrPrevious(moveNext.Value);
                    this.ArrangeElements();
                    this._isAnimating = false;
                }), moveNext.HasValue);
                this.ChangeCurrentInd(moveNext.Value);
            }
            else
            {
                if (delta == 0.0)
                    return;
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[0], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[1], delta, null, moveNext.HasValue);
                this.AnimateElementOnDragComplete((FrameworkElement)this._elements[2], delta, new Action(this.ArrangeElements), moveNext.HasValue);
            }
        }

        private void AnimateTwoElementsOnDragComplete(FrameworkElement element1, FrameworkElement element2, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int num = movingToNextOrPrevious ? 200 : 175;
            List<AnimationInfo> animInfoList = new List<AnimationInfo>();
            TranslateTransform translateTransform1 = element1.RenderTransform as TranslateTransform;
            TranslateTransform translateTransform2 = element2.RenderTransform as TranslateTransform;
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform1.X,
                to = translateTransform1.X + delta,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)translateTransform1,
                easing = GamesCatalogBannersSlideView.ANIMATION_EASING
            });
            animInfoList.Add(new AnimationInfo()
            {
                from = translateTransform2.X,
                to = translateTransform2.X + delta,
                propertyPath = (object)TranslateTransform.XProperty,
                duration = num,
                target = (DependencyObject)translateTransform2,
                easing = GamesCatalogBannersSlideView.ANIMATION_EASING
            });
            int? startTime = new int?(0);
            Action completed = completedCallback;
            AnimationUtil.AnimateSeveral(animInfoList, startTime, completed);
        }

        private void AnimateElementOnDragComplete(FrameworkElement element, double delta, Action completedCallback, bool movingToNextOrPrevious)
        {
            int duration = movingToNextOrPrevious ? 200 : 175;
            TranslateTransform target = element.RenderTransform as TranslateTransform;
            target.Animate(target.X, target.X + delta, (object)TranslateTransform.XProperty, duration, new int?(0), GamesCatalogBannersSlideView.ANIMATION_EASING, completedCallback);
        }

        private void AnimateElementVerticalOnDragComplete(FrameworkElement element, double to)
        {
            int duration = 200;
            TranslateTransform target = element.RenderTransform as TranslateTransform;
            target.Animate(target.Y, to, (object)TranslateTransform.YProperty, duration, new int?(0), GamesCatalogBannersSlideView.ANIMATION_EASING, null);
        }

        private void MoveToNextOrPrevious(bool next)
        {
            if (next)
            {
                GamesCatalogBannersSlideView.Swap(this._elements, 0, 1);
                GamesCatalogBannersSlideView.Swap(this._elements, 1, 2);
            }
            else
            {
                GamesCatalogBannersSlideView.Swap(this._elements, 1, 2);
                GamesCatalogBannersSlideView.Swap(this._elements, 0, 1);
            }
            this.UpdateSources(false, new bool?(next));
        }

        private void ChangeCurrentInd(bool next)
        {
            this._selectedIndex = !next ? this._selectedIndex - 1 : this._selectedIndex + 1;
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(this, this._selectedIndex);
            }
            this.OnPropertyChanged("SelectedIndex");
        }

        private static void Swap(List<Control> elements, int ind1, int ind2)
        {
            Control control = elements[ind1];
            elements[ind1] = elements[ind2];
            elements[ind2] = control;
        }

        public void EnableSwipe()
        {
            this.DisableSwipe();
            foreach (Control element in this._elements)
            {
                EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationStarted += eventHandler1;
                EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationDelta += eventHandler2;
                EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                element.ManipulationCompleted += eventHandler3;
            }
        }

        public void DisableSwipe()
        {
            foreach (Control element in this._elements)
            {
                EventHandler<ManipulationStartedEventArgs> eventHandler1 = new EventHandler<ManipulationStartedEventArgs>(this.Element_OnManipulationStarted);
                element.ManipulationStarted -= eventHandler1;
                EventHandler<ManipulationDeltaEventArgs> eventHandler2 = new EventHandler<ManipulationDeltaEventArgs>(this.Element_OnManipulationDelta);
                element.ManipulationDelta -= eventHandler2;
                EventHandler<ManipulationCompletedEventArgs> eventHandler3 = new EventHandler<ManipulationCompletedEventArgs>(this.Element_OnManipulationCompleted);
                element.ManipulationCompleted -= eventHandler3;
            }
        }

        public void PushScrollPosition(double sp)
        {
            if (!this.IsScrollListeningEnabled)
                return;
            if (sp > 0.0)
                this.DisableSwipe();
            else
                this.EnableSwipe();
        }

        private double CalculateOpacityFadeAwayBasedOnScroll(double sp)
        {
            return sp >= 10.0 ? (sp <= 60.0 ? 1.0 - (0.025 * sp - 0.25) : 0.0) : 1.0;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler changedEventHandler = this.PropertyChanged;
            if (changedEventHandler == null)
                return;
            changedEventHandler((object)this, new PropertyChangedEventArgs(propertyName));
        }

        [DebuggerNonUserCode]
        public void InitializeComponent()
        {
            if (this._contentLoaded)
                return;
            this._contentLoaded = true;
            Application.LoadComponent((object)this, new Uri("/VKClient.Common;component/UC/GamesCatalogBannersSlideView.xaml", UriKind.Relative));
            this.gridRoot = (Grid)this.FindName("gridRoot");
            this.LayoutRoot = (Grid)this.FindName("LayoutRoot");
        }
    }
}
