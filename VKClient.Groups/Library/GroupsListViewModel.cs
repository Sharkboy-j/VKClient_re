using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Groups.Localization;

namespace VKClient.Groups.Library
{
    public class GroupsListViewModel : ViewModelBase, IHandle<GroupMembershipStatusUpdated>, IHandle, IHandle<CountersChanged>, ICollectionDataProvider<List<Group>, GroupHeader>, ICollectionDataProvider<List<Group>, Group<GroupHeader>>
    {
        private long _uid;
        private bool _pickManaged;
        private string _userName;
        private GenericCollectionViewModel<List<Group>, GroupHeader> _allGroupsVM;
        private GenericCollectionViewModel<List<Group>, Group<GroupHeader>> _eventsVM;
        private GenericCollectionViewModel<List<Group>, GroupHeader> _managedVM;
        private AsyncHelper<BackendResult<GroupsLists, ResultCode>> _helperGroups;
        private CommunityInvitations _invitationsViewModel;
        private bool _groupsFetchCalledAtLeastOnce;
        private bool _managedFetchCalledAtLeastOnce;
        private bool _eventsFetchCalledAtLeastOnce;

        public GenericCollectionViewModel<List<Group>, GroupHeader> AllGroupsVM
        {
            get
            {
                return this._allGroupsVM;
            }
        }

        public GenericCollectionViewModel<List<Group>, Group<GroupHeader>> EventsVM
        {
            get
            {
                return this._eventsVM;
            }
        }

        public GenericCollectionViewModel<List<Group>, GroupHeader> ManagedVM
        {
            get
            {
                return this._managedVM;
            }
        }

        public bool OwnGroups
        {
            get
            {
                return this._uid == AppGlobalStateManager.Current.LoggedInUserId;
            }
        }

        public Visibility EventsCountVisibility
        {
            get
            {
                return !this.EventsVM.IsLoaded || this.EventsVM.Collection.Any<Group<GroupHeader>>() ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility InvitationsBlockVisibility
        {
            get
            {
                return !this.OwnGroups || this.InvitationsViewModel == null || this.InvitationsViewModel.count <= 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public CommunityInvitations InvitationsViewModel
        {
            get
            {
                return this._invitationsViewModel;
            }
            set
            {
                this._invitationsViewModel = value;
                this.NotifyPropertyChanged<CommunityInvitations>((System.Linq.Expressions.Expression<Func<CommunityInvitations>>)(() => this.InvitationsViewModel));
                this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.InvitationsBlockVisibility));
            }
        }

        public SolidColorBrush AllListBackground
        {
            get
            {
                if (this.AllGroupsVM.Collection.Count > 0 || this.InvitationsBlockVisibility == Visibility.Visible)
                    return (SolidColorBrush)Application.Current.Resources["PhoneRequestOrInvitationBackgroundBrush"];
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public SolidColorBrush ManageListBackground
        {
            get
            {
                if (this.ManagedVM.Collection.Count > 0)
                    return (SolidColorBrush)Application.Current.Resources["PhoneRequestOrInvitationBackgroundBrush"];
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public string Title
        {
            get
            {
                if (this._pickManaged)
                    return GroupResources.SELECTAGROUP;
                if (string.IsNullOrEmpty(this._userName))
                    return GroupResources.GroupsListPage_Title;
                return string.Format(GroupResources.GroupsListPage_TitleFrm, (object)this._userName).ToUpperInvariant();
            }
        }

        public Func<List<Group>, ListWithCount<GroupHeader>> ConverterFunc
        {
            get
            {
                return (Func<List<Group>, ListWithCount<GroupHeader>>)(groups => new ListWithCount<GroupHeader>()
                {
                    TotalCount = groups.Count,
                    List = new List<GroupHeader>(groups.Select<Group, GroupHeader>((Func<Group, GroupHeader>)(g => new GroupHeader(g, (User)null))))
                });
            }
        }

        Func<List<Group>, ListWithCount<Group<GroupHeader>>> ICollectionDataProvider<List<Group>, Group<GroupHeader>>.ConverterFunc
        {
            get
            {
                return (Func<List<Group>, ListWithCount<Group<GroupHeader>>>)(events =>
                {
                    List<GroupHeader> pastEvents = new List<GroupHeader>();
                    List<GroupHeader> futureEvents = new List<GroupHeader>();
                    events.ForEach((Action<Group>)(g =>
                    {
                        GroupHeader groupHeader = new GroupHeader(g, (User)null);
                        if (groupHeader.PastEvent)
                            pastEvents.Add(groupHeader);
                        else
                            futureEvents.Add(groupHeader);
                    }));
                    pastEvents = pastEvents.OrderBy<GroupHeader, int>((Func<GroupHeader, int>)(g => -g.Group.start_date)).ToList<GroupHeader>();
                    futureEvents = futureEvents.OrderBy<GroupHeader, int>((Func<GroupHeader, int>)(g => g.Group.start_date)).ToList<GroupHeader>();
                    ListWithCount<Group<GroupHeader>> listWithCount = new ListWithCount<Group<GroupHeader>>();
                    listWithCount.TotalCount = events.Count;
                    if (futureEvents.Count > 0)
                        listWithCount.List.Add(new Group<GroupHeader>(GroupResources.GroupsListPage_FutureEvents, (IEnumerable<GroupHeader>)futureEvents, false));
                    if (pastEvents.Count > 0)
                        listWithCount.List.Add(new Group<GroupHeader>(GroupResources.GroupsListPage_PastEvents, (IEnumerable<GroupHeader>)pastEvents, false));
                    return listWithCount;
                });
            }
        }

        public GroupsListViewModel(long uid, string userName = "", bool pickManaged = false)
        {
            this._uid = uid;
            this._userName = userName;
            this._pickManaged = pickManaged;
            EventAggregator.Current.Subscribe((object)this);
            this._allGroupsVM = new GenericCollectionViewModel<List<Group>, GroupHeader>((ICollectionDataProvider<List<Group>, GroupHeader>)this)
            {
                NoContentText = CommonResources.NoContent_Communities,
                NoContentImage = "../Resources/NoContentImages/Communities.png",
                NeedCollectionCountBeforeFullyLoading = true
            };
            this._eventsVM = new GenericCollectionViewModel<List<Group>, Group<GroupHeader>>((ICollectionDataProvider<List<Group>, Group<GroupHeader>>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true
            };
            this._managedVM = new GenericCollectionViewModel<List<Group>, GroupHeader>((ICollectionDataProvider<List<Group>, GroupHeader>)this)
            {
                NeedCollectionCountBeforeFullyLoading = true
            };
            this._helperGroups = new AsyncHelper<BackendResult<GroupsLists, ResultCode>>((Action<Action<BackendResult<GroupsLists, ResultCode>>>)(a => GroupsService.Current.GetUserGroups(this._uid, 0, 0, a)));
        }

        public void Handle(GroupMembershipStatusUpdated message)
        {
            Execute.ExecuteOnUIThread((Action)(() => this.LoadGroups(true, true)));
        }

        public void LoadGroups(bool refresh = true, bool suppressStatus = true)
        {
            CountersManager.Current.RefreshCounters();
            this._eventsVM.LoadData(refresh, suppressStatus, (Action<BackendResult<List<Group>, ResultCode>>)null, false);
            this._managedVM.LoadData(refresh, suppressStatus, (Action<BackendResult<List<Group>, ResultCode>>)null, false);
            this._allGroupsVM.LoadData(refresh, suppressStatus, (Action<BackendResult<List<Group>, ResultCode>>)null, false);
        }

        public void GetData(GenericCollectionViewModel<List<Group>, GroupHeader> caller, int offset, int count, Action<BackendResult<List<Group>, ResultCode>> callback)
        {
            if (caller == this._allGroupsVM)
            {
                this._helperGroups.RunAction((Action<BackendResult<GroupsLists, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                    {
                        callback(new BackendResult<List<Group>, ResultCode>(ResultCode.Succeeded)
                        {
                            ResultData = res.ResultData.Communities
                        });
                        this.InvitationsViewModel = res.ResultData.Invitations;
                        CountersManager.Current.Counters.groups = this.InvitationsViewModel.count;
                        EventAggregator.Current.Publish((object)new CountersChanged(CountersManager.Current.Counters));
                    }
                    else
                        callback(new BackendResult<List<Group>, ResultCode>(res.ResultCode));
                    this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.AllListBackground));
                }), this._groupsFetchCalledAtLeastOnce);
                this._groupsFetchCalledAtLeastOnce = true;
            }
            else
            {
                if (caller != this._managedVM)
                    return;
                this._helperGroups.RunAction((Action<BackendResult<GroupsLists, ResultCode>>)(res =>
                {
                    if (res.ResultCode == ResultCode.Succeeded)
                        callback(new BackendResult<List<Group>, ResultCode>(ResultCode.Succeeded)
                        {
                            ResultData = res.ResultData.AdminGroups
                        });
                    else
                        callback(new BackendResult<List<Group>, ResultCode>(res.ResultCode));
                    this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.ManageListBackground));
                }), this._managedFetchCalledAtLeastOnce);
                this._managedFetchCalledAtLeastOnce = true;
            }
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<Group>, GroupHeader> caller, int count)
        {
            if (count <= 0)
                return GroupResources.NoCommunites;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneCommunityFrm, GroupResources.TwoFourCommunitiesFrm, GroupResources.FiveCommunitiesFrm, true, (string)null, false);
        }

        public void GetData(GenericCollectionViewModel<List<Group>, Group<GroupHeader>> caller, int offset, int count, Action<BackendResult<List<Group>, ResultCode>> callback)
        {
            this._helperGroups.RunAction((Action<BackendResult<GroupsLists, ResultCode>>)(res =>
            {
                if (res.ResultCode == ResultCode.Succeeded)
                    callback(new BackendResult<List<Group>, ResultCode>(ResultCode.Succeeded)
                    {
                        ResultData = res.ResultData.Events
                    });
                else
                    callback(new BackendResult<List<Group>, ResultCode>(res.ResultCode));
            }), this._eventsFetchCalledAtLeastOnce);
            this._eventsFetchCalledAtLeastOnce = true;
        }

        public string GetFooterTextForCount(GenericCollectionViewModel<List<Group>, Group<GroupHeader>> caller, int count)
        {
            if (count <= 0)
                return GroupResources.NoEvents;
            return UIStringFormatterHelper.FormatNumberOfSomething(count, GroupResources.OneEventFrm, GroupResources.TwoFourEventsFrm, GroupResources.FiveEventsFrm, true, (string)null, false);
        }

        public void Handle(CountersChanged message)
        {
            this.NotifyPropertyChanged<CommunityInvitations>((System.Linq.Expressions.Expression<Func<CommunityInvitations>>)(() => this.InvitationsViewModel));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.InvitationsBlockVisibility));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.AllListBackground));
            this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>)(() => this.ManageListBackground));
            this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.EventsCountVisibility));
        }
    }
}
