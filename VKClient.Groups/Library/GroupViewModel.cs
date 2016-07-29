using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Profiles.Groups.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;
using VKMessenger.Library;

using VKClient.Audio.Base.Extensions;

namespace VKClient.Groups.Library
{
  public class GroupViewModel : ViewModelBase, ICollectionDataProvider<WallData, IVirtualizable>, IHandle<WallPostAddedOrEdited>, IHandle, IHandle<WallPostDeleted>, IHandle<WallPostPinnedUnpinned>, IHandle<WallPostPublished>, IHandle<WallPostPostponedPublished>, IHandle<WallPostSuggested>, IHandle<WallPostPostponed>, IHandle<GroupMembershipStatusUpdated>
  {
    private readonly string NO_AVATAR = "vk.com/images/community_200.png";
    private string _title = " ";
    private readonly ObservableCollection<ActionButton> _actionButtons = new ObservableCollection<ActionButton>();
    private readonly ObservableCollection<NavigateButton> _navigateButtons = new ObservableCollection<NavigateButton>();
    private readonly ObservableCollection<InformationRow> _infoRows = new ObservableCollection<InformationRow>();
    private bool _showAllPosts = true;
    private Visibility _actionButtonsSeparatorVisibility = Visibility.Collapsed;
    private readonly string _prefetchedName = "";
    private long _gid;
    private Group _group;
    private readonly GenericCollectionViewModel<WallData, IVirtualizable> _wallVM;
    private bool _isLoading;
    private bool _loaded;
    private GroupMembershipInfo _groupMembershipInfo;

    public GenericCollectionViewModel<WallData, IVirtualizable> WallVM
    {
      get
      {
        return this._wallVM;
      }
    }

    public bool ShowAllPosts
    {
      get
      {
        return this._showAllPosts;
      }
      set
      {
        if (this._showAllPosts == value || this._wallVM.IsInProgress)
          return;
        this._showAllPosts = value;
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.AllPostsOpacity));
        this.NotifyPropertyChanged<double>((System.Linq.Expressions.Expression<Func<double>>) (() => this.GroupPostsOpacity));
        DelayedExecutorUtil.Execute((Action) (() => this.WallVM.LoadData(true, false, (Action<BackendResult<WallData, ResultCode>>) null, false)), 50);
      }
    }

    public double AllPostsOpacity
    {
      get
      {
        return !this._showAllPosts ? 0.4 : 1.0;
      }
    }

    public double GroupPostsOpacity
    {
      get
      {
        return this._showAllPosts ? 0.4 : 1.0;
      }
    }

    public string AllPostsText
    {
      get
      {
        return CommonResources.Group_AllPosts.ToUpperInvariant();
      }
    }

    public string GroupPostsText
    {
      get
      {
        return CommonResources.Group_CommunityPosts.ToUpperInvariant();
      }
    }

    public Visibility AllVSGroupPostsVisibility
    {
      get
      {
        return this._group == null || this._group.GroupType == GroupType.PublicPage || !this._group.CanPost ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public Visibility AllVSGroupPostsVisibilityInversed
    {
      get
      {
        return this.AllVSGroupPostsVisibility != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Group Group
    {
      get
      {
        return this._group;
      }
    }

    public string GroupName
    {
      get
      {
        if (this._group == null)
          return "";
        return this._group.name;
      }
    }

    public bool IsFavorite
    {
      get
      {
        if (this._group != null)
          return this._group.IsFavorite;
        return false;
      }
    }

    public bool IsSubscribed
    {
      get
      {
        if (this._group != null)
          return this._group.IsSubscribed;
        return false;
      }
    }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = (value ?? "").ToUpperInvariant();
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
      }
    }

    public string Avatar
    {
      get
      {
        if (this._group == null)
          return "";
        return this._group.photo_200;
      }
    }

    public bool HaveAvatar
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.Avatar))
          return !this.Avatar.Contains(this.NO_AVATAR);
        return false;
      }
    }

    public Visibility IsVerifiedVisibility
    {
      get
      {
        return this._group == null || !this._group.IsVerified ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool CanPost
    {
      get
      {
        if (this._group != null)
          return this._group.CanPost;
        return false;
      }
    }

    public bool CanSuggestAPost
    {
      get
      {
        if (!this.CanPost && this._group != null)
          return this._group.GroupType == GroupType.PublicPage;
        return false;
      }
    }

    public string GroupTypeStr
    {
      get
      {
        if (this._group == null || !this._loaded)
          return "";
        switch (this._group.GroupType)
        {
          case GroupType.Group:
            return GroupHeader.GetGroupTypeText(this._group).Capitalize();
          case GroupType.PublicPage:
            return GroupResources.PublicPage.Capitalize();
          case GroupType.Event:
            return GroupResources.Event.Capitalize();
          default:
            return "";
        }
      }
    }

    public string GroupText
    {
      get
      {
        if (this._group == null || !this._loaded)
          return "";
        if (this._group.GroupType == GroupType.PublicPage || this._group.GroupType == GroupType.Group)
          return this._group.link;
        return this.GetGroupTextForEvent();
      }
    }

    public ObservableCollection<ActionButton> ActionButtons
    {
      get
      {
        return this._actionButtons;
      }
    }

    public Visibility ActionButtonsSeparatorVisibility
    {
      get
      {
        return this._actionButtonsSeparatorVisibility;
      }
      set
      {
        this._actionButtonsSeparatorVisibility = value;
        this.NotifyPropertyChanged("ActionButtonsSeparatorVisibility");
      }
    }

    public ObservableCollection<NavigateButton> NavigateButtons
    {
      get
      {
        return this._navigateButtons;
      }
    }

    public ObservableCollection<InformationRow> InformationRows
    {
      get
      {
        return this._infoRows;
      }
    }

    public long Gid
    {
      get
      {
        return this._gid;
      }
    }

    public GroupMembershipInfo GroupMembershipInfo
    {
      get
      {
        return this._groupMembershipInfo;
      }
      set
      {
        this._groupMembershipInfo = value;
        this.NotifyPropertyChanged("GroupMembershipInfo");
      }
    }

    public Func<WallData, ListWithCount<IVirtualizable>> ConverterFunc
    {
      get
      {
        return (Func<WallData, ListWithCount<IVirtualizable>>) (wallData =>
        {
          ListWithCount<IVirtualizable> listWithCount = new ListWithCount<IVirtualizable>();
          List<IVirtualizable> virtualizableList = WallPostItemsGenerator.Generate(wallData.wall, wallData.profiles, wallData.groups, new Action<WallPostItem>(this.DeletedCallback), 0.0);
          listWithCount.List.AddRange((IEnumerable<IVirtualizable>) virtualizableList);
          int totalCount = wallData.TotalCount;
          listWithCount.TotalCount = totalCount;
          return listWithCount;
        });
      }
    }

    public GroupViewModel(long gid, string name)
    {
      this._gid = gid;
      if (name != string.Empty)
      {
        this._prefetchedName = name;
        this.Title = name;
      }
      this._wallVM = new GenericCollectionViewModel<WallData, IVirtualizable>((ICollectionDataProvider<WallData, IVirtualizable>) this)
      {
        LoadCount = 5,
        ReloadCount = 20
      };
      EventAggregator.Current.Subscribe((object) this);
      EventAggregator.Current.Publish((object) new OpenGroupEvent()
      {
        GroupId = this._gid
      });
    }

    public void LoadGroupData(bool reload = true, bool suppressLoading = true)
    {
      if (this._isLoading)
        return;
      this._isLoading = true;
      this.SetInProgress(true, reload ? CommonResources.Refreshing : CommonResources.Loading);
      Execute.ExecuteOnUIThread((Action) (() => GroupsService.Current.GetGroupInfo(this._gid, (Action<BackendResult<GroupData, ResultCode>>) (res =>
      {
        this.SetInProgress(false, "");
        this._isLoading = false;
        this._loaded = true;
        if (res.ResultCode != ResultCode.Succeeded)
          return;
        this._group = res.ResultData.group;
        this.ReadData();
        this.LoadWallData(reload, suppressLoading);
      }))));
    }

    public void LoadWallData(bool reload = false, bool suppressMessage = true)
    {
      this.WallVM.LoadData(reload, suppressMessage, (Action<BackendResult<WallData, ResultCode>>) null, false);
    }

    private void DeletedCallback(WallPostItem obj)
    {
      this.WallVM.Delete((IVirtualizable) obj);
    }

    public void FaveUnfave()
    {
      bool add = !this.IsFavorite;
      FavoritesService.Instance.FaveAddRemoveGroup(this._gid, add, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, add ? CommonResources.Bookmarks_CommunityIsAdded : CommonResources.Bookmarks_CommunityIsRemoved, (VKRequestsDispatcher.Error) null);
        if (res.ResultCode != ResultCode.Succeeded || this._group == null)
          return;
        this._group.IsFavorite = !this._group.IsFavorite;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsFavorite));
        EventAggregator current = EventAggregator.Current;
        GroupFavedUnfavedEvent favedUnfavedEvent = new GroupFavedUnfavedEvent();
        favedUnfavedEvent.group = this._group;
        int num = this.IsFavorite ? 1 : 0;
        favedUnfavedEvent.IsFaved = num != 0;
        current.Publish((object) favedUnfavedEvent);
      }));
    }

    public void SubscribeUnsubscribe()
    {
      if (!this.IsSubscribed)
      {
        WallService.Current.WallSubscriptionsSubscribe(-this._gid, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.NewsPostsNotificationsAreEnabled, (VKRequestsDispatcher.Error) null);
          if (res.ResultCode != ResultCode.Succeeded || this._group == null)
            return;
          this._group.IsSubscribed = !this._group.IsSubscribed;
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSubscribed));
          EventAggregator current = EventAggregator.Current;
          GroupSubscribedUnsubscribedEvent unsubscribedEvent = new GroupSubscribedUnsubscribedEvent();
          unsubscribedEvent.group = this._group;
          int num = this.IsSubscribed ? 1 : 0;
          unsubscribedEvent.IsSubscribed = num != 0;
          current.Publish((object) unsubscribedEvent);
        }));
      }
      else
      {
        WallService current1 = WallService.Current;
        List<long> ownerIds = new List<long>();
        ownerIds.Add(-this._gid);
        Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
        {
          GenericInfoUC.ShowBasedOnResult((int) res.ResultCode, CommonResources.NewsPostsNotificationsAreDisabled, (VKRequestsDispatcher.Error) null);
          if (res.ResultCode != ResultCode.Succeeded || this._group == null)
            return;
          this._group.IsSubscribed = !this._group.IsSubscribed;
          this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSubscribed));
          EventAggregator current = EventAggregator.Current;
          GroupSubscribedUnsubscribedEvent unsubscribedEvent = new GroupSubscribedUnsubscribedEvent();
          unsubscribedEvent.group = this._group;
          int num = this.IsSubscribed ? 1 : 0;
          unsubscribedEvent.IsSubscribed = num != 0;
          current.Publish((object) unsubscribedEvent);
        });
        current1.WallSubscriptionsUnsubscribe(ownerIds, callback);
      }
    }

    private void ReadData()
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Avatar));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.GroupTypeStr));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.GroupText));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsVerifiedVisibility));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsFavorite));
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSubscribed));
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.GroupName));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.AllVSGroupPostsVisibility));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.AllVSGroupPostsVisibilityInversed));
        if (this._group != null)
          this.Title = this._group.name;
        this.ActionButtonsSeparatorVisibility = Visibility.Visible;
        this.ReadActionButtons();
        this.ReadNavigateButtons();
        this.ReadInfoRows();
        this.GroupMembershipInfo = new GroupMembershipInfo((GroupData) null);
      }));
    }

    private void ReadInfoRows()
    {
      this._infoRows.Clear();
      if (!string.IsNullOrEmpty(this._group.description))
        this._infoRows.Add((InformationRow) new DescriptionInformationRow(this._group.description));
      if (!string.IsNullOrEmpty(this._group.link))
        this._infoRows.Add((InformationRow) new WebLinkInformationRow(this._group.link));
      if (string.IsNullOrEmpty(this._group.screen_name))
        return;
      this._infoRows.Add((InformationRow) new WebLinkInformationRow("http://vk.com/" + this._group.screen_name));
    }

    private void ReadNavigateButtons()
    {
      this._navigateButtons.Clear();
      if (this._group.members_count > 0)
      {
        NavigateButton navigateButton1 = new NavigateButton();
        navigateButton1.ButtonType = NavigateButtonType.Subscribers;
        string str = UIStringFormatterHelper.FormatForUIShort((long) this._group.members_count);
        navigateButton1.ButtonTitle = str;
        NavigateButton navigateButton2 = navigateButton1;
        navigateButton2.ButtonSubtitle = this._group.GroupType != GroupType.PublicPage ? UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, GroupResources.OneMemberFrm, GroupResources.TwoFourMembersFrm, GroupResources.FiveMembersFrm, false, null, false) : UIStringFormatterHelper.FormatNumberOfSomething(this._group.members_count, GroupResources.OneSubscriberFrm, GroupResources.TwoFourSubscribersFrm, GroupResources.FiveSubscribersFrm, false, null, false);
        this._navigateButtons.Add(navigateButton2);
      }
      if (this._group.counters.topics > 0)
      {
        NavigateButton navigateButton = new NavigateButton();
        navigateButton.ButtonType = NavigateButtonType.Discussions;
        string str1 = UIStringFormatterHelper.FormatForUIShort((long) this._group.counters.topics);
        navigateButton.ButtonTitle = str1;
        string str2 = UIStringFormatterHelper.FormatNumberOfSomething(this._group.counters.topics, GroupResources.OneTopicFrm, GroupResources.TwoFourTopicsFrm, GroupResources.FiveTopicsFrm, false, null, false);
        navigateButton.ButtonSubtitle = str2;
        this._navigateButtons.Add(navigateButton);
      }
      if (this._group.counters.photos > 0)
      {
        NavigateButton navigateButton = new NavigateButton();
        navigateButton.ButtonType = NavigateButtonType.Photo;
        string str1 = UIStringFormatterHelper.FormatForUIShort((long) this._group.counters.photos);
        navigateButton.ButtonTitle = str1;
        string str2 = this._group.counters.photos == 1 ? GroupResources.Photo : GroupResources.Photos;
        navigateButton.ButtonSubtitle = str2;
        this._navigateButtons.Add(navigateButton);
      }
      if (this._group.counters.videos > 0)
      {
        NavigateButton navigateButton = new NavigateButton();
        navigateButton.ButtonType = NavigateButtonType.Video;
        string str1 = UIStringFormatterHelper.FormatForUIShort((long) this._group.counters.videos);
        navigateButton.ButtonTitle = str1;
        string str2 = this._group.counters.videos == 1 ? GroupResources.Video : GroupResources.Videos;
        navigateButton.ButtonSubtitle = str2;
        this._navigateButtons.Add(navigateButton);
      }
      if (this._group.counters.audios > 0)
      {
        NavigateButton navigateButton = new NavigateButton();
        navigateButton.ButtonType = NavigateButtonType.Audio;
        string str1 = UIStringFormatterHelper.FormatForUIShort((long) this._group.counters.audios);
        navigateButton.ButtonTitle = str1;
        string str2 = this._group.counters.audios == 1 ? GroupResources.Audio : GroupResources.Audios;
        navigateButton.ButtonSubtitle = str2;
        this._navigateButtons.Add(navigateButton);
      }
      if (this._group.counters.docs <= 0)
        return;
      NavigateButton navigateButton3 = new NavigateButton();
      navigateButton3.ButtonType = NavigateButtonType.Documents;
      string str3 = UIStringFormatterHelper.FormatForUIShort((long) this._group.counters.docs);
      navigateButton3.ButtonTitle = str3;
      string str4 = UIStringFormatterHelper.FormatNumberOfSomething(this._group.counters.docs, CommonResources.OneDocFrm, CommonResources.TwoFourDocumentsFrm, CommonResources.FiveDocumentsFrm, false, null, false);
      navigateButton3.ButtonSubtitle = str4;
      this._navigateButtons.Add(navigateButton3);
    }

    public void HandleNavigateButton(NavigateButtonType buttonType)
    {
      switch (buttonType)
      {
        case NavigateButtonType.Subscribers:
          Navigator.Current.NavigateToCommunitySubscribers(this._gid, this._group.GroupType, false, false, false);
          break;
        case NavigateButtonType.Photo:
          Navigator.Current.NavigateToPhotoAlbums(false, this._gid, true, this._group.admin_level);
          break;
        case NavigateButtonType.Video:
          Navigator.Current.NavigateToVideo(false, this._gid, true, this._group != null && this._group.CanUploadVideo);
          break;
        case NavigateButtonType.Audio:
          Navigator.Current.NavigateToAudio(0, this._gid, true, 0L, 0L, "");
          break;
        case NavigateButtonType.Discussions:
          Navigator.Current.NavigateToGroupDiscussions(this._gid, this._group.name, this._group.admin_level, this._group.GroupType == GroupType.PublicPage, this._group.CanCreateTopic);
          break;
        case NavigateButtonType.Documents:
          Navigator.Current.NavigateToDocuments(-this._gid, this._group.admin_level > 1);
          break;
      }
    }

    private void ReadActionButtons()
    {
      this._actionButtons.Clear();
      if (this._group.PostponedPostsCount > 0)
        this._actionButtons.Add(new ActionButton(UIStringFormatterHelper.FormatNumberOfSomething(this._group.PostponedPostsCount, CommonResources.PostponedNews_OnePostponedPostFrm, CommonResources.PostponedNews_TwoFourPostponedPostsFrm, CommonResources.PostponedNews_FivePostponedPostsFrm, true, null, false), ActionButtonType.Postponed));
      if (this._group.SuggestedPostsCount > 0)
        this._actionButtons.Add(new ActionButton(UIStringFormatterHelper.FormatNumberOfSomething(this._group.SuggestedPostsCount, CommonResources.SuggestedNews_OneSuggestedPostFrm, CommonResources.SuggestedNews_TwoFourSuggestedPostsFrm, CommonResources.SuggestedNews_FiveSuggestedPostsFrm, true, null, false), ActionButtonType.Suggested));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.ActionButtonsSeparatorVisibility));
    }

    public void HandleActionButton(ActionButtonType buttonType)
    {
      switch (buttonType)
      {
        case ActionButtonType.Join:
        case ActionButtonType.MayBe:
          GroupsService.Current.Join(this._gid, buttonType == ActionButtonType.MayBe, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
          {
            if (res.ResultCode != ResultCode.Succeeded)
              return;
            CountersManager.Current.Counters = res.ResultData;
            if (this._group.MembershipType == GroupMembershipType.NotAMember && this._group.Privacy == GroupPrivacy.Public)
              EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, true));
            else if (this._group.MembershipType == GroupMembershipType.InvitationReceived)
              EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, true));
            this.LoadGroupData(true, false);
          }));
          break;
        case ActionButtonType.Leave:
          GroupsService.Current.Leave(this._gid, (Action<BackendResult<OwnCounters, ResultCode>>) (res =>
          {
            if (res.ResultCode != ResultCode.Succeeded)
              return;
            CountersManager.Current.Counters = res.ResultData;
            if (this._group.MembershipType == GroupMembershipType.Member)
              EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, false));
            else if (this._group.MembershipType == GroupMembershipType.InvitationReceived)
              EventAggregator.Current.Publish((object) new GroupMembershipStatusUpdated(this._group.id, false));
            this.LoadGroupData(true, false);
          }));
          break;
        case ActionButtonType.WriteOnWall:
          Navigator.Current.NavigateToNewWallPost(this._gid, true, this._group.admin_level, this._group.GroupType == GroupType.PublicPage, false, false);
          break;
        case ActionButtonType.Map:
          Navigator.Current.NavigateToMap(false, this._group.place.latitude, this._group.place.longitude);
          break;
        case ActionButtonType.Suggested:
          Navigator.Current.NavigateToSuggestedPostponedPostsPage(this._gid, true, 0);
          break;
        case ActionButtonType.Postponed:
          Navigator.Current.NavigateToSuggestedPostponedPostsPage(this._gid, true, 1);
          break;
      }
    }

    private string GetGroupTextForEvent()
    {
      string str1 = "";
      if (this._group.start_date == 0)
        return "";
      string str2 = str1 + UIStringFormatterHelper.FormateDateForEventUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double) this._group.start_date, false));
      if (this._group.finish_date != 0)
        str2 = str2 + " — " + UIStringFormatterHelper.FormateDateForEventUI(VKClient.Common.Utils.Extensions.UnixTimeStampToDateTime((double) this._group.finish_date, false));
      if (this._group.place != null)
      {
        if (!string.IsNullOrEmpty(this._group.place.title))
          str2 = str2 + ", " + this._group.place.title;
        if (!string.IsNullOrEmpty(this._group.place.address))
          str2 = str2 + ", " + this._group.place.address;
        if (!string.IsNullOrEmpty(this._group.place.cityName))
          str2 = str2 + ", " + this._group.place.cityName;
        if (!string.IsNullOrEmpty(this._group.place.countryName))
          str2 = str2 + ", " + this._group.place.countryName;
      }
      return str2;
    }

    public void GetData(GenericCollectionViewModel<WallData, IVirtualizable> caller, int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      WallService.Current.GetWall(-this._gid, offset, count, callback, this._showAllPosts ? "all" : "owner");
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<WallData, IVirtualizable> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoWallPosts;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneWallPostFrm, CommonResources.TwoWallPostsFrm, CommonResources.FiveWallPostsFrm, true, null, false);
    }

    public void Handle(WallPostDeleted message)
    {
      WallPostItem wallPostItem = this.WallVM.Collection.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (w =>
      {
        if (w is WallPostItem && (w as WallPostItem).WallPost.to_id == message.WallPost.to_id)
          return (w as WallPostItem).WallPost.id == message.WallPost.id;
        return false;
      })) as WallPostItem;
      if (wallPostItem != null)
        this.DeletedCallback(wallPostItem);
      if (message.WallPost.post_type == "suggest")
      {
        --this._group.SuggestedPostsCount;
        this.ReadActionButtons();
      }
      else
      {
        if (!message.WallPost.IsPostponed)
          return;
        --this._group.PostponedPostsCount;
        this.ReadActionButtons();
      }
    }

    public void Handle(WallPostAddedOrEdited message)
    {
      if (message.NewlyAddedWallPost.to_id != -this._gid || message.NewlyAddedWallPost.IsPostponed)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        List<WallPost> wallPosts = new List<WallPost>();
        wallPosts.Add(message.NewlyAddedWallPost);
        List<User> users = message.Users;
        List<Group> groups = message.Groups;
        Action<WallPostItem> deletedCallback = new Action<WallPostItem>(this.DeletedCallback);
        double itemsWidth = 0.0;
        IVirtualizable virtualizable1 = WallPostItemsGenerator.Generate(wallPosts, users, groups, deletedCallback, itemsWidth).First<IVirtualizable>();
        IVirtualizable virtualizable2 = this.WallVM.Collection.FirstOrDefault<IVirtualizable>((Func<IVirtualizable, bool>) (wp =>
        {
          if (wp is WallPostItem && ((WallPostItem) wp).WallPost.id == message.NewlyAddedWallPost.id)
            return ((WallPostItem) wp).WallPost.to_id == message.NewlyAddedWallPost.to_id;
          return false;
        }));
        if (virtualizable2 == null)
        {
          bool flag = this.WallVM.Collection.Any<IVirtualizable>() && (this.WallVM.Collection.First<IVirtualizable>() as WallPostItem).WallPost.is_pinned == 1;
          this.WallVM.Insert(virtualizable1, flag ? 1 : 0);
        }
        else
        {
          int ind = this.WallVM.Collection.IndexOf(virtualizable2);
          this.WallVM.Delete(virtualizable2);
          this.WallVM.Insert(virtualizable1, ind);
        }
      }));
    }

    internal void PinToStart()
    {
      if ((this._group == null || string.IsNullOrWhiteSpace(this._group.name)) && string.IsNullOrWhiteSpace(this._prefetchedName) || this._isLoading)
        return;
      this._isLoading = true;
      string smallPhoto = "";
      if (this._group != null && this._group.photo_200 != null)
        smallPhoto = this._group.photo_200;
      string name = "";
      if (!string.IsNullOrWhiteSpace(this._prefetchedName))
        name = this._prefetchedName;
      if (this._group != null)
        name = this._group.name;
      this.SetInProgress(true, "");
      SecondaryTileCreator.CreateTileFor(this._gid, true, name, (Action<bool>) (res =>
      {
        this.SetInProgress(false, "");
        this._isLoading = false;
        if (res)
          return;
        Execute.ExecuteOnUIThread((Action) (() => ExtendedMessageBox.ShowSafe(CommonResources.Error)));
      }), smallPhoto);
    }

    public void Handle(WallPostPinnedUnpinned message)
    {
      if (message.OwnerId != -this._gid)
        return;
      this.LoadGroupData(true, false);
    }

    public void Handle(WallPostPublished message)
    {
      if (message.WallPost.to_id != -this._gid || this._group == null)
        return;
      if (message.IsSuggested)
        --this._group.SuggestedPostsCount;
      if (message.IsPostponed)
        ++this._group.PostponedPostsCount;
      this.ReadActionButtons();
    }

    public void Handle(WallPostPostponedPublished message)
    {
      --this._group.PostponedPostsCount;
      this.ReadActionButtons();
    }

    public void Handle(WallPostSuggested message)
    {
      if (message.to_id != -this._gid || this._group == null)
        return;
      ++this._group.SuggestedPostsCount;
      this.ReadActionButtons();
    }

    public void Handle(WallPostPostponed message)
    {
      if (this._group == null)
        return;
      ++this._group.PostponedPostsCount;
      this.ReadActionButtons();
    }

    public void Handle(GroupMembershipStatusUpdated message)
    {
      this.LoadGroupData(true, false);
    }
  }
}
