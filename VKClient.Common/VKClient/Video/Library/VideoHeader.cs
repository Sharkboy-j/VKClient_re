using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Video.Library
{
  public class VideoHeader : ViewModelBase, IHandle<VideoEdited>, IHandle, IHaveUniqueKey, ISearchableItemHeader<VKClient.Common.Backend.DataObjects.Video>
  {
    private ObservableCollection<MenuItemData> _menuItems = new ObservableCollection<MenuItemData>();
    private List<User> _knownUsers = new List<User>();
    private List<Group> _knownGroups = new List<Group>();
    private string _context = "";
    private VKClient.Common.Backend.DataObjects.Video _vKVideo;
    private bool _pickMode;
    private long _albumId;
    private bool _fromSearch;
    private StatisticsActionSource _actionSource;

    public ObservableCollection<MenuItemData> MenuItems
    {
      get
      {
        return this._menuItems;
      }
    }

    public Visibility AllowEditVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility AllowDeleteVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility AllowEditOrDeleteVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public Visibility IsLiveVisibility
    {
      get
      {
        return this._vKVideo.live != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility ShowPlaySmallIconVisibility
    {
      get
      {
        return this.ShowDurationVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility ShowDurationVisibility
    {
      get
      {
        return string.IsNullOrWhiteSpace(this.UIDuration) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility AlreadyViewedVisibility
    {
      get
      {
        return this._vKVideo.watched != 1 ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string Title
    {
      get
      {
        return this._vKVideo.title;
      }
    }

    public Visibility IsVideoVisibility
    {
      get
      {
        return Visibility.Visible;
      }
    }

    public Visibility IsAlbumVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public string CountStr
    {
      get
      {
        return "";
      }
    }

    public string Subtitle1
    {
      get
      {
        if (this._vKVideo.owner_id < 0L)
        {
          Group group = this._knownGroups.FirstOrDefault<Group>((Func<Group, bool>) (g => g.id == -this._vKVideo.owner_id));
          if (group != null)
            return group.name;
        }
        else
        {
          User user = this._knownUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.id == this._vKVideo.owner_id));
          if (user != null)
            return user.Name;
        }
        return "";
      }
    }

    public string Subtitle2
    {
      get
      {
        int views = this._vKVideo.views;
        if (views <= 0)
          return "";
        return UIStringFormatterHelper.FormatNumberOfSomething(views, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true, null, false);
      }
    }

    public string ImageUri
    {
      get
      {
        return this._vKVideo.photo_320;
      }
    }

    public string ViewsString
    {
      get
      {
        int number = this._vKVideo != null ? this._vKVideo.views : 0;
        if (number <= 0)
          return "";
        return UIStringFormatterHelper.FormatNumberOfSomething(number, CommonResources.OneViewFrm, CommonResources.TwoFourViewsFrm, CommonResources.FiveViewsFrm, true, null, false);
      }
    }

    public bool CanPlay
    {
      get
      {
        return VideoPlayerHelper.CanPlayVideo(this._vKVideo);
      }
    }

    public Visibility CannotPlayVisibility
    {
      get
      {
        return !this.CanPlay ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility NoVideosVisibility
    {
      get
      {
        return Visibility.Collapsed;
      }
    }

    public bool IsMenuEnabled
    {
      get
      {
        return this._menuItems.Count > 0;
      }
    }

    public VKClient.Common.Backend.DataObjects.Video VKVideo
    {
      get
      {
        return this._vKVideo;
      }
      private set
      {
        this._vKVideo = value;
        this.NotifyPropertyChanged<VKClient.Common.Backend.DataObjects.Video>((System.Linq.Expressions.Expression<Func<VKClient.Common.Backend.DataObjects.Video>>) (() => this.VKVideo));
      }
    }

    public string Image
    {
      get
      {
        if (this.VKVideo == null)
          return string.Empty;
        if (!string.IsNullOrEmpty(this.VKVideo.image_medium))
          return this.VKVideo.image_medium;
        if (!string.IsNullOrEmpty(this.VKVideo.image))
          return this.VKVideo.image;
        return string.Empty;
      }
    }

    public string VideoUri { get; set; }

    public bool IsExternal { get; set; }

    public string UIDuration
    {
      get
      {
        if (this._vKVideo.live == 1)
          return CommonResources.VideoCatalog_LIVE;
        if (this._vKVideo.duration <= 0)
          return "";
        return UIStringFormatterHelper.FormatDuration(this._vKVideo.duration);
      }
    }

    public bool FromSearch
    {
      get
      {
        return this._fromSearch;
      }
      set
      {
        this._fromSearch = value;
      }
    }

    public bool IsLocalItem
    {
      get
      {
        return this.VKVideo.owner_id == AppGlobalStateManager.Current.LoggedInUserId;
      }
    }

    public VideoHeader(VKClient.Common.Backend.DataObjects.Video video, List<MenuItemData> menuItems = null, List<User> knownUsers = null, List<Group> knownGroups = null, StatisticsActionSource source = StatisticsActionSource.undefined, string context = "", bool pickMode = false, long albumId = 0)
    {
      this.VKVideo = video;
      this.SetMenuItems(menuItems);
      this._knownUsers = knownUsers ?? new List<User>();
      this._knownGroups = knownGroups ?? new List<Group>();
      this._pickMode = pickMode;
      this._albumId = albumId;
      this._actionSource = source;
      this._context = context;
      EventAggregator.Current.Subscribe((object) this);
    }

    public void SetMenuItems(List<MenuItemData> menuItems)
    {
      this._menuItems.Clear();
      if (menuItems == null)
        return;
      menuItems.ForEach((Action<MenuItemData>) (m => this._menuItems.Add(m)));
    }

    public void Handle(VideoEdited message)
    {
      if (this._vKVideo == null || message.Video.id != this._vKVideo.id || message.Video.owner_id != this._vKVideo.owner_id)
        return;
      this._vKVideo.title = message.Video.title;
      this._vKVideo.description = message.Video.description;
      this._vKVideo.privacy_view = message.Video.privacy_view;
      this._vKVideo.privacy_comment = message.Video.privacy_comment;
      this.VKVideo = this._vKVideo;
    }

    public string GetKey()
    {
      if (this._vKVideo == null)
        return "";
      return this._vKVideo.owner_id.ToString() + "_" + (object) this._vKVideo.vid;
    }

    public bool Matches(string searchString)
    {
      return this.VKVideo.title.ToLowerInvariant().Contains(searchString.ToLowerInvariant());
    }

    public void HandleTap()
    {
      if (this._fromSearch)
        return;
      if (!this._pickMode)
      {
        CurrentMediaSource.VideoSource = this._actionSource;
        CurrentMediaSource.VideoContext = this._context;
        Navigator.Current.NavigateToVideoWithComments(this._vKVideo, this._vKVideo.owner_id, this._vKVideo.vid, this._vKVideo.access_key);
      }
      else
      {
        ParametersRepository.SetParameterForId("PickedVideo", (object) this._vKVideo);
        if (this._albumId != 0L)
        {
          PageBase currentPage = FramePageUtils.CurrentPage;
          if (currentPage != null)
            currentPage.NavigationService.RemoveBackEntrySafe();
        }
        Navigator.Current.GoBack();
      }
    }
  }
}
