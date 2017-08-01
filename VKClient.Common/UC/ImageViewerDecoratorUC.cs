using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using VKClient.Audio.Base;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.ImageViewer;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Photos.Library;

namespace VKClient.Common.UC
{
  public class ImageViewerDecoratorUC : UserControl, INotifyPropertyChanged, IHandle<PhotoIsRepostedInGroup>, IHandle, IHandle<PhotoViewerOrientationLockedModeChanged>
  {
    private bool _fromDialog;
    private bool _friendsOnly;
    private readonly DispatcherTimer _timer;
    private bool _cancelBackKeyPress;
    private DialogService _ds;
    private SharePostUC _sharePostUC;
    private ImageViewerViewModel _imageViewerVM;
    private bool _isShown;
    private IApplicationBar _savedPageAppBar;
    private double _actualWidth;
    private double _actualHeight;
    private bool _userHideDecorator;
    private System.Windows.Controls.Page _initialPage;
    private Dictionary<FrameworkElement, bool> _elementsVisibilityDict;
    private Func<int, Image> _getImageByIdFunc;
    private FrameworkElement _currentViewControl;
    private Action<int> _setContextOnViewControl;
    private Action<int, bool> _showHideOverlay;
    private bool _hideActions;
    private PhoneApplicationPage _page;
    private const int TEXT_MARGIN_BOTTOM = 80;
    private const int TEXT_MARGIN_TOP = 112;
    internal VKClient.Common.ImageViewer.ImageViewer imageViewer;
    internal Canvas gridTop;
    internal Image rectBlackTop;
    internal TextBlock textBlockCounter;
    internal Border borderActions;
    internal Border borderOrientationLock;
    internal ContextMenu contextMenu;
    internal MenuItem menuItemSaveInAlbum;
    internal Border borderMore;
    internal Canvas gridBottom;
    internal Image rectBlackBottom;
    internal ScrollableTextBlock textBoxText;
    internal StackPanel stackPanelLikesComments;
    internal Border borderLikes;
    internal Border borderComments;
    internal Grid gridGifs;
    internal Border borderShare;
    internal Border borderMarks;
    private bool _contentLoaded;

    public bool CancelBackKeyPress
    {
      get
      {
        return this._cancelBackKeyPress;
      }
      set
      {
        this._cancelBackKeyPress = value;
      }
    }

    public FrameworkElement GridBottom
    {
      get
      {
        return (FrameworkElement) this.gridBottom;
      }
    }

    public bool IsShown
    {
      get
      {
        return this._isShown;
      }
      private set
      {
        if (this._isShown == value)
          return;
        EventHandler<bool> isShownChanging = this.IsShownChanging;
        if (isShownChanging != null)
        {
          int num = value ? 1 : 0;
          isShownChanging(this, num != 0);
        }
        this._isShown = value;
      }
    }

    private ImageViewerViewModel ImageViewerVM
    {
      get
      {
        return this._imageViewerVM;
      }
      set
      {
        if (this._imageViewerVM == value)
          return;
        this._imageViewerVM = value;
      }
    }

    public PhotoViewModel CurrentPhotoVM
    {
      get
      {
        return this.GetVMByIndex(this.imageViewer.CurrentInd);
      }
    }

    public PhotoViewModel NextPhotoVM
    {
      get
      {
        return this.GetVMByIndex(this.imageViewer.CurrentInd + 1);
      }
    }

    public PhotoViewModel PreviousPhotoVM
    {
      get
      {
        return this.GetVMByIndex(this.imageViewer.CurrentInd - 1);
      }
    }

    private PhoneApplicationPage Page
    {
      get
      {
        PhoneApplicationPage page = this._page;
        if (page != null)
          return page;
        PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
        return this._page = (rootVisual != null ? ((ContentControl) rootVisual).Content : null) as PhoneApplicationPage;
      }
    }

    public event EventHandler<bool> IsShownChanging;

    public event PropertyChangedEventHandler PropertyChanged;

    public ImageViewerDecoratorUC()
    {
      DispatcherTimer dispatcherTimer = new DispatcherTimer();
      TimeSpan timeSpan = TimeSpan.FromMilliseconds(200.0);
      dispatcherTimer.Interval = timeSpan;
      this._timer = dispatcherTimer;
      this._cancelBackKeyPress = true;
      this._ds = new DialogService();
      //base.\u002Ector();
      this.InitializeComponent();
      base.Visibility = Visibility.Collapsed;
      this.imageViewer.SupportOrientationChange = !AppGlobalStateManager.Current.GlobalState.IsPhotoViewerOrientationLocked;
      this.imageViewer.HideCallback = (Action) (() => this.Hide(false));
      this.imageViewer.TapCallback = new Action(this.RespondToTap);
      this.imageViewer.DoubleTapCallback = new Action(this.RespondToDoubleTap);
      this.imageViewer.CurrentIndexChanged = new Action(this.RespondToCurrentIndexChanged);
      this.imageViewer.ManuallyAppliedOrientationChanged = (Action) (() => this.RespondToManuallyAppliedOrientationChanged(false));
      this.imageViewer.IsInVerticalSwipeChanged = new Action(this.RespondToVertSwipeChange);
      ((FrameworkElement) this.imageViewer).Height = this.imageViewer.HARDCODED_HEIGHT;
      EventAggregator.Current.Subscribe(this);
      base.DataContext = this;
      // ISSUE: method pointer
      base.SizeChanged += (new SizeChangedEventHandler( this.OnSizeChanged));
    }

    private PhotoViewModel GetVMByIndex(int currentInd)
    {
      if (currentInd >= 0 && this.ImageViewerVM != null && currentInd < this.ImageViewerVM.PhotosCollection.Count)
        return this.ImageViewerVM.PhotosCollection[currentInd];
      return  null;
    }

    private void DoShare()
    {
      if (this.CurrentPhotoVM == null)
        return;
      this._ds = new DialogService()
      {
        SetStatusBarBackground = false,
        HideOnNavigation = false
      };
      this._sharePostUC = new SharePostUC(0L);
      this._sharePostUC.ShareCommunityTap += new EventHandler(this.ButtonShareWithCommunity_Click);
      this._sharePostUC.SendTap += new EventHandler(this.ButtonSend_Click);
      this._sharePostUC.ShareTap += new EventHandler(this.ButtonShare_Click);
      if (this._fromDialog || this._friendsOnly)
      {
        this._sharePostUC.SetShareEnabled(false);
        this._sharePostUC.SetShareCommunityEnabled(false);
      }
      this._ds.Child = (FrameworkElement) this._sharePostUC;
      this._ds.AnimationType = DialogService.AnimationTypes.None;
      this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      this._ds.Show( null);
    }

    private void ButtonShareWithCommunity_Click(object sender, EventArgs eventArgs)
    {
      if (this.CurrentPhotoVM == null)
        return;
      string str = UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text);
      INavigator current = Navigator.Current;
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      string name = "";
      int num1 = 1;
      Doc document1 = this.CurrentPhotoVM.Document;
      long owner_id = document1 != null ? document1.owner_id : this.CurrentPhotoVM.OwnerId;
      Doc document2 = this.CurrentPhotoVM.Document;
      long pic_id = document2 != null ? document2.id : this.CurrentPhotoVM.Pid;
      string text = str;
      int num2 = this.CurrentPhotoVM.IsGif ? 1 : 0;
      string accessKey = this.CurrentPhotoVM.Document == null ? "" : this.CurrentPhotoVM.Document.access_key;
      long excludedId = 0;
      current.NavigateToGroups(loggedInUserId, name, num1 != 0, owner_id, pic_id, text, num2 != 0, accessKey, excludedId);
    }

    private void ButtonShare_Click(object sender, EventArgs eventArgs)
    {
      this.Share(0, "");
    }

    private void Share(long gid = 0, string groupName = "")
    {
      if (this.CurrentPhotoVM == null)
        return;
      this._ds.Hide();
      this.CurrentPhotoVM.Share(UIStringFormatterHelper.CorrectNewLineCharacters(this._sharePostUC.Text), gid, groupName);
    }

    private void ButtonSend_Click(object sender, EventArgs eventArgs)
    {
      PhotoViewModel currentPhotoVm1 = this.CurrentPhotoVM;
      if ((currentPhotoVm1 != null ? currentPhotoVm1.Photo :  null) == null)
      {
        PhotoViewModel currentPhotoVm2 = this.CurrentPhotoVM;
        if ((currentPhotoVm2 != null ? currentPhotoVm2.Document :  null) == null)
          return;
      }
      this._ds.Hide();
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.Message = this._sharePostUC.Text;
      contentDataProvider.Photo = this.CurrentPhotoVM.Document == null ? this.CurrentPhotoVM.Photo :  null;
      contentDataProvider.Document = this.CurrentPhotoVM.Document;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
      Navigator.Current.NavigateToPickConversation();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args)
    {
        int num1 = this.Page.Orientation == PageOrientation.LandscapeLeft ? 1 : (this.Page.Orientation == PageOrientation.LandscapeRight ? 1 : 0);
      Size newSize = args.NewSize;
      // ISSUE: explicit reference operation
      double num2 = ((Size) @newSize).Width;
      // ISSUE: explicit reference operation
      double num3 = ((Size) @newSize).Height;
      if (num1 != 0)
      {
        double num4 = num2;
        num2 = num3;
        num3 = num4;
      }
      this._actualWidth = num2;
      this._actualHeight = num3;
    }

    private void RespondToTap()
    {
      this._timer.Tick+=(new EventHandler(this._timer_Tick));
      this._timer.Start();
    }

    private void _timer_Tick(object sender, EventArgs e)
    {
      this._timer.Stop();
      this._timer.Tick-=(new EventHandler(this._timer_Tick));
      this._userHideDecorator = !this._userHideDecorator;
      this.Update();
    }

    private void RespondToDoubleTap()
    {
      this._timer.Stop();
      this._timer.Tick-=(new EventHandler(this._timer_Tick));
    }

    private void RespondToVertSwipeChange()
    {
      this.Update();
    }

    private void RespondToManuallyAppliedOrientationChanged(bool force = false)
    {
        if (!this.imageViewer.SupportOrientationChange && !force || this.Page.SupportedOrientations == SupportedPageOrientation.PortraitOrLandscape)
        return;
      this.UpdateRotationValues();
      this.Update();
    }

    private void UpdateRotationValues()
    {
      if (this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeLeft)
      {
        CompositeTransform compositeTransform1 = new CompositeTransform();
        double num1 = 90.0;
        compositeTransform1.Rotation = num1;
        double actualWidth = this._actualWidth;
        compositeTransform1.TranslateX = actualWidth;
        ((UIElement) this.gridTop).RenderTransform = ((Transform) compositeTransform1);
        CompositeTransform compositeTransform2 = new CompositeTransform();
        double num2 = 90.0;
        compositeTransform2.Rotation = num2;
        double height = ((FrameworkElement) this.rectBlackBottom).Height;
        compositeTransform2.TranslateX = height;
        double num3 = -this._actualHeight + ((FrameworkElement) this.rectBlackBottom).Height;
        compositeTransform2.TranslateY = num3;
        CompositeTransform compositeTransform3 = compositeTransform2;
        ((FrameworkElement)this.gridBottom).Margin = new Thickness();
        ((UIElement) this.gridBottom).RenderTransform = ((Transform) compositeTransform3);
      }
      else if (this.imageViewer.ManuallyAppliedOrientation == DeviceOrientation.LandscapeRight)
      {
        CompositeTransform compositeTransform1 = new CompositeTransform();
        double num1 = -90.0;
        compositeTransform1.Rotation = num1;
        double actualHeight = this._actualHeight;
        compositeTransform1.TranslateY = actualHeight;
        ((UIElement) this.gridTop).RenderTransform = ((Transform) compositeTransform1);
        CompositeTransform compositeTransform2 = new CompositeTransform();
        double num2 = -90.0;
        compositeTransform2.Rotation = num2;
        double num3 = this._actualWidth - ((FrameworkElement) this.rectBlackBottom).Height;
        compositeTransform2.TranslateX = num3;
        double height = ((FrameworkElement) this.rectBlackBottom).Height;
        compositeTransform2.TranslateY = height;
        CompositeTransform compositeTransform3 = compositeTransform2;
        ((FrameworkElement)this.gridBottom).Margin = new Thickness();
        ((UIElement) this.gridBottom).RenderTransform = ((Transform) compositeTransform3);
      }
      else
      {
        ((UIElement) this.gridBottom).RenderTransform = ((Transform) new CompositeTransform());
        double num = 0.0;
        FrameworkElement content = ((UserControl) this.Page).Content as FrameworkElement;
        if (content != null)
        {
          Thickness margin = content.Margin;
          // ISSUE: explicit reference operation
          num = ((Thickness) @margin).Bottom;
        }
        ((FrameworkElement) this.gridBottom).Margin=(new Thickness(0.0, 0.0, 0.0, -num));
        ((UIElement) this.gridTop).RenderTransform = ((Transform) new CompositeTransform());
      }
    }

    private void RespondToCurrentIndexChanged()
    {
      // ISSUE: reference to a compiler-generated field
      PropertyChangedEventHandler propertyChanged1 = this.PropertyChanged;
      if (propertyChanged1 != null)
      {
        PropertyChangedEventArgs e = new PropertyChangedEventArgs("CurrentPhotoVM");
        propertyChanged1(this, e);
      }
      this.UpdateMenuItems();
      if (this.CurrentPhotoVM != null)
      {
        Action loadPreviousVM = (Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (this.PreviousPhotoVM == null || this.PreviousPhotoVM.IsLoadedFullInfo)
            return;
          this.PreviousPhotoVM.LoadInfoWithComments((Action<bool, int>) ((resPrevious, re) => {}));
        })));
        Action loadNextPhotoVM = (Action) (() => Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.Update();
          if (this.NextPhotoVM == null)
            return;
          if (!this.NextPhotoVM.IsLoadedFullInfo)
            this.NextPhotoVM.LoadInfoWithComments((Action<bool, int>) ((resNext, ress) => loadPreviousVM()));
          else
            loadPreviousVM();
        })));
        if (!this.CurrentPhotoVM.IsLoadedFullInfo)
          this.CurrentPhotoVM.LoadInfoWithComments((Action<bool, int>) ((res, r) => loadNextPhotoVM()));
        else
          loadNextPhotoVM();
      }
      this.imageViewer.ForbidResizeInNormalMode = this.CurrentPhotoVM != null && this.CurrentPhotoVM.IsGif;
      if (this._imageViewerVM.PhotosCollection.Count - 1 - this.imageViewer.CurrentInd < 3)
        this.ImageViewerVM.LoadMorePhotos((Action<bool>) (res =>
        {
          if (!res)
            return;
          this.HandlePhotoUpdate(this.ImageViewerVM);
          // ISSUE: reference to a compiler-generated field
          PropertyChangedEventHandler propertyChanged2 = this.PropertyChanged;
          if (propertyChanged2 != null)
          {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs("CurrentPhotoVM");
            propertyChanged2(this, e);
          }
          if (this.CurrentPhotoVM != null && !this.CurrentPhotoVM.IsLoadedFullInfo)
            this.CurrentPhotoVM.LoadInfoWithComments((Action<bool, int>) ((r, r1) => Execute.ExecuteOnUIThread(new Action(this.Update))));
          this.Update();
        }));
      this.Update();
    }

    private void UpdateMenuItems()
    {
      ((UIElement) this.menuItemSaveInAlbum).Visibility = ((this.CurrentPhotoVM == null || this.CurrentPhotoVM.OwnerId != AppGlobalStateManager.Current.LoggedInUserId).ToVisiblity());
    }

    private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
    {
      this.UpdateRotationValues();
      this.Update();
    }

    private void Show(int ind)
    {
      base.Visibility = Visibility.Visible;
      this._savedPageAppBar = this.Page.ApplicationBar;
      this._userHideDecorator = false;
      PhoneApplicationPage page = this.Page;
      this._initialPage = (System.Windows.Controls.Page) this.Page;
      ((UIElement) this.borderMore).Visibility = ((!this._hideActions).ToVisiblity());
      // ISSUE: variable of the null type
      
      page.ApplicationBar = null;
      EventHandler<OrientationChangedEventArgs> eventHandler1 = new EventHandler<OrientationChangedEventArgs>(this.Page_OrientationChanged);
      page.OrientationChanged += (eventHandler1);
      EventHandler<CancelEventArgs> eventHandler2 = new EventHandler<CancelEventArgs>(this.Page_BackKeyPress);
      page.BackKeyPress += (eventHandler2);
      if (page.SupportedOrientations != SupportedPageOrientation.Portrait)
        ((UIElement) this.borderOrientationLock).Visibility = Visibility.Collapsed;
      this.IsShown = true;
      this.RespondToManuallyAppliedOrientationChanged(false);
      this.imageViewer.Show(ind, new Action(this.Update), false,  null);
    }

    public void Hide(bool leavingPageImmediately = false)
    {
      if (!leavingPageImmediately)
      {
        this.Page.OrientationChanged -= (new EventHandler<OrientationChangedEventArgs>(this.Page_OrientationChanged));
        this.Page.BackKeyPress -= (new EventHandler<CancelEventArgs>(this.Page_BackKeyPress));
        this.Page.ApplicationBar = this._savedPageAppBar;
      }
      this.IsShown = false;
      if (!leavingPageImmediately)
        this.Update();
      this.imageViewer.Hide((Action) (() => base.Visibility = Visibility.Collapsed), leavingPageImmediately);
    }

    private void Page_BackKeyPress(object sender, CancelEventArgs e)
    {
      if (this._ds.IsOpen)
        return;
      if (this._cancelBackKeyPress)
        e.Cancel = true;
      this.Hide(!this._cancelBackKeyPress);
    }

    private void Update()
    {
      if (this.Page != this._initialPage)
        return;
      if (!this.imageViewer.IsInVerticalSwipe && this.IsShown && !this._userHideDecorator)
      {
        ((UIElement) this.textBlockCounter).Visibility = Visibility.Visible;
        ((UIElement) this.borderActions).Visibility = Visibility.Visible;
        ((UIElement) this.stackPanelLikesComments).Visibility = Visibility.Visible;
        if (this.CurrentPhotoVM != null && this.CurrentPhotoVM.IsGif)
        {
          ((UIElement) this.borderLikes).Visibility = Visibility.Collapsed;
          ((UIElement) this.borderComments).Visibility = Visibility.Collapsed;
          ((UIElement) this.gridGifs).Visibility = Visibility.Visible;
        }
        else
        {
          ((UIElement) this.borderLikes).Visibility = Visibility.Visible;
          ((UIElement) this.borderComments).Visibility = Visibility.Visible;
          ((UIElement) this.gridGifs).Visibility = Visibility.Collapsed;
        }
        bool isPortrait = (!this.imageViewer.SupportOrientationChange || this.imageViewer.ManuallyAppliedOrientation != DeviceOrientation.LandscapeRight && this.imageViewer.ManuallyAppliedOrientation != DeviceOrientation.LandscapeLeft) && this.Page.Orientation != PageOrientation.LandscapeLeft && this.Page.Orientation != PageOrientation.LandscapeRight;
        this.UpdateActionsBarPosition(isPortrait);
        this.UpdateTextWidth(isPortrait);
        this.UpdateActionsWidth(isPortrait);
        ((UIElement) this.borderMore).Visibility = (this.CurrentPhotoVM == null || !isPortrait || this.CurrentPhotoVM.IsGif ? Visibility.Collapsed : (Visibility) (int) (!this._hideActions).ToVisiblity());
        ((UIElement) this.rectBlackBottom).Visibility = Visibility.Visible;
        ((UIElement) this.rectBlackTop).Visibility = Visibility.Visible;
        ((UIElement) this.textBoxText).Visibility = Visibility.Visible;
      }
      else
      {
        if (this.IsShown)
          this.Page.ApplicationBar = ( null);
        ((UIElement) this.rectBlackBottom).Visibility = Visibility.Collapsed;
        ((UIElement) this.rectBlackTop).Visibility = Visibility.Collapsed;
        ((UIElement) this.textBlockCounter).Visibility = Visibility.Collapsed;
        ((UIElement) this.borderActions).Visibility = Visibility.Collapsed;
        ((UIElement) this.stackPanelLikesComments).Visibility = Visibility.Collapsed;
        ((UIElement) this.gridGifs).Visibility = Visibility.Collapsed;
        ((UIElement) this.textBoxText).Visibility = Visibility.Collapsed;
      }
      if (this.ImageViewerVM.PhotosCount > 0)
        this.textBlockCounter.Text = (string.Format(CommonResources.ImageViewer_PhotoCounterFrm, (this.ImageViewerVM.PhotosCollection.IndexOf(this.CurrentPhotoVM) + 1 + this.ImageViewerVM.InitialOffset), this.ImageViewerVM.PhotosCount));
      else
        this.textBlockCounter.Text = ("");
    }

    private void UpdateActionsBarPosition(bool isPortrait)
    {
      Canvas.SetLeft((UIElement) this.borderActions, (isPortrait ? this._actualWidth : this._actualHeight) - ((FrameworkElement) this.borderActions).Width);
    }

    private void UpdateTextWidth(bool isPortrait)
    {
      ((FrameworkElement) this.textBoxText).Width=(Math.Max(0.0, (isPortrait ? this._actualWidth : this._actualHeight) - 32.0));
    }

    private void UpdateActionsWidth(bool isPortrait)
    {
      if (this._elementsVisibilityDict == null)
      {
        Dictionary<FrameworkElement, bool> dictionary = new Dictionary<FrameworkElement, bool>();
        FrameworkElement borderLikes = (FrameworkElement) this.borderLikes;
        int num1 = 0;
        dictionary[borderLikes] = num1 != 0;
        FrameworkElement borderComments = (FrameworkElement) this.borderComments;
        int num2 = 0;
        dictionary[borderComments] = num2 != 0;
        FrameworkElement borderShare = (FrameworkElement) this.borderShare;
        int num3 = 0;
        dictionary[borderShare] = num3 != 0;
        FrameworkElement borderMarks = (FrameworkElement) this.borderMarks;
        int num4 = 0;
        dictionary[borderMarks] = num4 != 0;
        FrameworkElement gridGifs = (FrameworkElement) this.gridGifs;
        int num5 = 0;
        dictionary[gridGifs] = num5 != 0;
        this._elementsVisibilityDict = dictionary;
      }
      else
      {
        List<FrameworkElement>.Enumerator enumerator = ((List<FrameworkElement>) Enumerable.ToList<FrameworkElement>(this._elementsVisibilityDict.Keys)).GetEnumerator();
        try
        {
          while (enumerator.MoveNext())
            this._elementsVisibilityDict[enumerator.Current] = false;
        }
        finally
        {
          enumerator.Dispose();
        }
      }
      if (this.CurrentPhotoVM == null)
        return;
      if (!this.CurrentPhotoVM.IsGif)
      {
        this._elementsVisibilityDict[(FrameworkElement) this.borderLikes] = true;
        this._elementsVisibilityDict[(FrameworkElement) this.borderComments] = true;
        if (this.CurrentPhotoVM.IsUserVisible)
          this._elementsVisibilityDict[(FrameworkElement) this.borderMarks] = true;
      }
      else
        this._elementsVisibilityDict[(FrameworkElement) this.gridGifs] = true;
      if (isPortrait)
        this._elementsVisibilityDict[(FrameworkElement) this.borderShare] = true;
      int num6 = Enumerable.Count<FrameworkElement>(this._elementsVisibilityDict.Keys, (Func<FrameworkElement, bool>)(element => this._elementsVisibilityDict[element]));
      double num7 = (isPortrait ? this._actualWidth : this._actualHeight) / (double) num6;
      Dictionary<FrameworkElement, bool>.KeyCollection.Enumerator enumerator1 = this._elementsVisibilityDict.Keys.GetEnumerator();
      try
      {
        while (enumerator1.MoveNext())
        {
          FrameworkElement current = enumerator1.Current;
          current.Width = num7;
          ((UIElement) current).Visibility = (this._elementsVisibilityDict[current] ? Visibility.Visible : Visibility.Collapsed);
        }
      }
      finally
      {
        enumerator1.Dispose();
      }
    }

    private void InitWith(ImageViewerViewModel ivvm, Func<int, Image> getImageByIdFunc, FrameworkElement currentViewControl = null, Action<int> setContextOnViewControl = null, Action<int, bool> showHideOverlay = null)
    {
      this._imageViewerVM = ivvm;
      this._getImageByIdFunc = getImageByIdFunc;
      this._currentViewControl = currentViewControl;
      this._setContextOnViewControl = setContextOnViewControl;
      this._showHideOverlay = showHideOverlay;
      this.HandlePhotoUpdate(ivvm);
    }

    private void HandlePhotoUpdate(ImageViewerViewModel ivvm)
    {
      if (this._imageViewerVM != ivvm)
        return;
      this.imageViewer.Initialize(ivvm.PhotosCollection.Count, (Func<int, ImageInfo>) (ind =>
      {
        Func<int, Image> getImageByIdFunc = this._getImageByIdFunc;
        Image image1;
        if (getImageByIdFunc == null)
        {
          image1 =  null;
        }
        else
        {
          int num = ind;
          image1 = getImageByIdFunc(num);
        }
        Image image2 = image1;
        ImageInfo imageInfo = new ImageInfo();
        if (ind == this.imageViewer.CurrentInd && this.imageViewer.CurrentImage.Source is BitmapImage)
        {
          BitmapImage source = this.imageViewer.CurrentImage.Source as BitmapImage;
          imageInfo.Width = (double) ((BitmapSource) source).PixelWidth;
          imageInfo.Height = (double) ((BitmapSource) source).PixelHeight;
        }
        if ((image2 != null ? image2.Source :  null) is BitmapImage)
        {
          BitmapImage source = (BitmapImage) image2.Source;
          imageInfo.Width = (double) ((BitmapSource) source).PixelWidth;
          imageInfo.Height = (double) ((BitmapSource) source).PixelHeight;
        }
        if (ind < 0 || ind >= ivvm.PhotosCollection.Count)
          return  null;
        PhotoViewModel photos = ivvm.PhotosCollection[ind];
        if (imageInfo.Width == 0.0)
          imageInfo.Width = (double) photos.Photo.width;
        if (imageInfo.Height == 0.0)
          imageInfo.Height = (double) photos.Photo.height;
        imageInfo.Uri = photos.ImageSrc;
        return imageInfo;
      }), this._getImageByIdFunc, (Action<int, bool>) ((ind, show) =>
      {
        Func<int, Image> getImageByIdFunc = this._getImageByIdFunc;
        Image image1;
        if (getImageByIdFunc == null)
        {
          image1 =  null;
        }
        else
        {
          int num = ind;
          image1 = getImageByIdFunc(num);
        }
        Image image2 = image1;
        if (image2 != null)
          ((UIElement) image2).Opacity = (show ? 1.0 : 0.0);
        Action<int, bool> showHideOverlay = this._showHideOverlay;
        if (showHideOverlay == null)
          return;
        int num1 = ind;
        int num2 = show ? 1 : 0;
        showHideOverlay(num1, num2 != 0);
      }), this._currentViewControl, this._setContextOnViewControl);
    }

    public static void ShowPhotosFromFeed(long userOrGroupId, bool isGroup, string aid, int photosCount, int selectedPhotoIndex, int date, List<Photo> photos, string mode, Func<int, Image> getImageByIdFunc)
    {
      // ISSUE: type reference
      ViewerMode mode1 = (ViewerMode) Enum.Parse(typeof (ViewerMode), mode);
      ImageViewerViewModel ivvm = new ImageViewerViewModel(userOrGroupId, isGroup, aid, photosCount, date, photos, mode1);
      ImageViewerDecoratorUC dec = ImageViewerDecoratorUC.GetDecoratorForCurrentPage( null);
      dec.InitWith(ivvm, getImageByIdFunc,  null,  null,  null);
      ivvm.LoadPhotosFromFeed((Action<bool>) (res =>
      {
        if (!res)
          return;
        Execute.ExecuteOnUIThread((Action) (() => dec.HandlePhotoUpdate(ivvm)));
      }));
      dec.Show(selectedPhotoIndex);
    }

    public static void ShowPhotosOrGifsById(int selectedPhotoIndex, List<PhotoOrDocument> photoOrDocList, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false, FrameworkElement currentViewControl = null, Action<int> setContextOnCurrentViewControl = null, Action<int, bool> showHideOverlay = null, bool shareButtonOnly = false)
    {
      ImageViewerViewModel ivvm = new ImageViewerViewModel(photoOrDocList);
      ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(page);
      decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc, currentViewControl, setContextOnCurrentViewControl, showHideOverlay);
      decoratorForCurrentPage._fromDialog = fromDialog;
      decoratorForCurrentPage._friendsOnly = friendsOnly;
      decoratorForCurrentPage._hideActions = hideActions;
      if (hideActions)
        ((UIElement) decoratorForCurrentPage.gridBottom).Visibility = Visibility.Collapsed;
      decoratorForCurrentPage.Show(selectedPhotoIndex);
    }

    public static void ShowPhotosById(int photosCount, int initialOffset, int selectedPhotoIndex, List<long> photoIds, List<long> ownerIds, List<string> accessKeys, List<Photo> photos, bool fromDialog = false, bool friendsOnly = false, Func<int, Image> getImageByIdFunc = null, PageBase page = null, bool hideActions = false)
    {
      ViewerMode mode = ViewerMode.PhotosByIds;
      List<Photo> photos1 = photos;
      ImageViewerViewModel ivvm = new ImageViewerViewModel(photosCount, initialOffset, photoIds, ownerIds, accessKeys, photos1, mode);
      ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage(page);
      decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc,  null,  null,  null);
      decoratorForCurrentPage._fromDialog = fromDialog;
      decoratorForCurrentPage._friendsOnly = friendsOnly;
      decoratorForCurrentPage._hideActions = hideActions;
      if (hideActions)
        ((UIElement) decoratorForCurrentPage.gridBottom).Visibility = Visibility.Collapsed;
      decoratorForCurrentPage.Show(selectedPhotoIndex);
    }

    public static void ShowPhotosFromAlbum(string aid, int albumType, long userOrGroupId, bool isGroup, int photosCount, int selectedPhotoIndex, List<Photo> photos, Func<int, Image> getImageByIdFunc)
    {
      ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(aid, (AlbumType) albumType, userOrGroupId, isGroup, photosCount, photos);
      ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage( null);
      ImageViewerViewModel ivvm = imageViewerViewModel;
      Func<int, Image> getImageByIdFunc1 = getImageByIdFunc;
      // ISSUE: variable of the null type
      // ISSUE: variable of the null type
      
      // ISSUE: variable of the null type

      decoratorForCurrentPage.InitWith(ivvm, getImageByIdFunc1, null, null, null);
      int ind = selectedPhotoIndex;
      decoratorForCurrentPage.Show(ind);
    }

    public static void ShowPhotosFromProfile(long userOrGroupId, bool isGroup, int selectedPhotoIndex, List<Photo> photos, bool canLoadMoreProfileListPhotos)
    {
      ImageViewerViewModel imageViewerViewModel = new ImageViewerViewModel(userOrGroupId, isGroup, photos, canLoadMoreProfileListPhotos, 0L);
      ImageViewerDecoratorUC decoratorForCurrentPage = ImageViewerDecoratorUC.GetDecoratorForCurrentPage( null);
      ImageViewerViewModel ivvm = imageViewerViewModel;
      // ISSUE: variable of the null type
      // ISSUE: variable of the null type
      
      // ISSUE: variable of the null type

      decoratorForCurrentPage.InitWith(ivvm, (Func<int, Image>)(i => null), null, null, null);
      int ind = selectedPhotoIndex;
      decoratorForCurrentPage.Show(ind);
    }

    private static ImageViewerDecoratorUC GetDecoratorForCurrentPage(PageBase page = null)
    {
      if (page == null)
      {
        PhoneApplicationFrame rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
        page = (rootVisual != null ? ((ContentControl) rootVisual).Content : null) as PageBase;
      }
      if (page == null)
        return  null;
      return page.ImageViewerDecorator;
    }

    public void SetPage(PhoneApplicationPage page)
    {
      this._page = page;
      this.imageViewer.SetPage(this._page);
    }

    private void SaveImage(BitmapImage bitmap)
    {
      ImageHelper.SaveImage(bitmap);
    }

    private void ShowConfirmation()
    {
      new GenericInfoUC().ShowAndHideLater(CommonResources.PhotoIsSaved,  null);
    }

    private void LikeUnlikeTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      PhotoViewModel currentPhotoVm = this.CurrentPhotoVM;
      if (currentPhotoVm == null)
        return;
      currentPhotoVm.LikeUnlike();
    }

    private void CommentTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.NavigateToComments();
    }

    private void UserTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.NavigateToComments();
    }

    private void NavigateToComments()
    {
      if (this.CurrentPhotoVM == null)
        return;
      Navigator.Current.NavigateToPhotoWithComments(this.CurrentPhotoVM.Photo, this.CurrentPhotoVM.PhotoWithInfo, this.CurrentPhotoVM.Photo.owner_id, this.CurrentPhotoVM.Photo.pid, this.CurrentPhotoVM.AccessKey, this._fromDialog, this._friendsOnly);
    }

    private void textBoxTextTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.NavigateToComments();
    }

    public void Handle(PhotoIsRepostedInGroup message)
    {
      if (!this._ds.IsOpen)
        return;
      this._ds.Hide();
    }

    private void LayoutRoot_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void LayoutRoot_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void LayoutRoot_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    private void AddTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      PhotoViewModel currentPhotoVm = this.CurrentPhotoVM;
      if (currentPhotoVm == null)
        return;
      currentPhotoVm.AddDocument();
    }

    private void ShareTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.DoShare();
    }

    private void OrientationLockMode_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      PhotoViewModel currentPhotoVm = this.CurrentPhotoVM;
      if (currentPhotoVm == null)
        return;
      currentPhotoVm.ToggleOrientationLockMode();
    }

    public void Handle(PhotoViewerOrientationLockedModeChanged message)
    {
      this.imageViewer.ResetOrientation();
      this.RespondToManuallyAppliedOrientationChanged(true);
    }

    private void TextBoxText_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size newSize = e.NewSize;
      // ISSUE: explicit reference operation
      double num1 = ((Size) @newSize).Height;
      if (string.IsNullOrEmpty(this.textBoxText.Text))
        num1 = 0.0;
      double num2 = Math.Max(0.0, num1 + 192.0);
      ((FrameworkElement) this.rectBlackBottom).Height = num2;
      ((FrameworkElement) this.gridBottom).Height = num2;
      this.UpdateRectBlackBottomTransform();
      if (!this.imageViewer.SupportOrientationChange)
        return;
      this.UpdateRotationValues();
    }

    private void UpdateRectBlackBottomTransform()
    {
      RotateTransform renderTransform = ((UIElement) this.rectBlackBottom).RenderTransform as RotateTransform;
      if (renderTransform == null)
        return;
      renderTransform.CenterX=(((FrameworkElement) this.rectBlackBottom).Width / 2.0);
      renderTransform.CenterY=(((FrameworkElement) this.rectBlackBottom).Height / 2.0);
    }

    private void BorderMore_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ShowMoreOptions();
    }

    private void ShowMoreOptions()
    {
      this.contextMenu.IsOpen = true;
    }

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e)
    {
      BitmapImage source = this.imageViewer.CurrentImage.Source as BitmapImage;
      if (source == null || ((BitmapSource) source).PixelWidth == 0)
        return;
      this.SaveImage(source);
    }

    private void MenuItemSaveInAlbum_OnClick(object sender, RoutedEventArgs e)
    {
      PhotoViewModel currentPhotoVm = this.CurrentPhotoVM;
      if (currentPhotoVm == null)
        return;
      currentPhotoVm.SaveInSavedPhotosAlbum();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/ImageViewerDecoratorUC.xaml", UriKind.Relative));
      this.imageViewer = (VKClient.Common.ImageViewer.ImageViewer) base.FindName("imageViewer");
      this.gridTop = (Canvas) base.FindName("gridTop");
      this.rectBlackTop = (Image) base.FindName("rectBlackTop");
      this.textBlockCounter = (TextBlock) base.FindName("textBlockCounter");
      this.borderActions = (Border) base.FindName("borderActions");
      this.borderOrientationLock = (Border) base.FindName("borderOrientationLock");
      this.contextMenu = (ContextMenu) base.FindName("contextMenu");
      this.menuItemSaveInAlbum = (MenuItem) base.FindName("menuItemSaveInAlbum");
      this.borderMore = (Border) base.FindName("borderMore");
      this.gridBottom = (Canvas) base.FindName("gridBottom");
      this.rectBlackBottom = (Image) base.FindName("rectBlackBottom");
      this.textBoxText = (ScrollableTextBlock) base.FindName("textBoxText");
      this.stackPanelLikesComments = (StackPanel) base.FindName("stackPanelLikesComments");
      this.borderLikes = (Border) base.FindName("borderLikes");
      this.borderComments = (Border) base.FindName("borderComments");
      this.gridGifs = (Grid) base.FindName("gridGifs");
      this.borderShare = (Border) base.FindName("borderShare");
      this.borderMarks = (Border) base.FindName("borderMarks");
    }
  }
}
