using System;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Groups.Management.Library
{
  public sealed class RequestsViewModel : ViewModelBase, ICollectionDataProvider<VKList<User>, FriendHeader>
  {
    public readonly long CommunityId;

    public GenericCollectionViewModel<VKList<User>, FriendHeader> Requests { get; set; }//

    public Func<VKList<User>, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<User>, ListWithCount<FriendHeader>>) (list =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>();
          listWithCount.TotalCount = list.count;
          foreach (User user in list.items)
            listWithCount.List.Add(new FriendHeader(user, false));
          return listWithCount;
        });
      }
    }

    public RequestsViewModel(long communityId)
    {
      this.CommunityId = communityId;
      this.Requests = new GenericCollectionViewModel<VKList<User>, FriendHeader>((ICollectionDataProvider<VKList<User>, FriendHeader>) this)
      {
        LoadCount = 60,
        ReloadCount = 100
      };
    }

    public void GetData(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      GroupsService.Current.GetRequests(this.CommunityId, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<User>, FriendHeader> caller, int count)
    {
      if (count <= 0)
        return CommonResources.NoRequests;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.RequestOneForm, CommonResources.RequestTwoForm, CommonResources.RequestFiveForm, true, null, false);
    }

    public void HandleRequest(FriendHeader item, bool isAcception)
    {
      this.SetInProgress(true, "");
      GroupsService.Current.HandleRequest(this.CommunityId, item.UserId, isAcception, (Action<BackendResult<int, ResultCode>>) (result => Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
          this.Requests.Delete(item);
        else
          GenericInfoUC.ShowBasedOnResult((int) result.ResultCode, "", (VKRequestsDispatcher.Error) null);
        this.SetInProgress(false, "");
      }))));
    }
  }
}
