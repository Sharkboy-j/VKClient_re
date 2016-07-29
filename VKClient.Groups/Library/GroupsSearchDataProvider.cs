using System;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKClient.Groups.Library
{
  public class GroupsSearchDataProvider : ISearchDataProvider<Group, GroupHeader>
  {
    public string LocalGroupName
    {
      get
      {
        return "";
      }
    }

    public string GlobalGroupName
    {
      get
      {
        return CommonResources.GlobalSearch.ToUpperInvariant();
      }
    }

    public IEnumerable<GroupHeader> LocalItems { get; set; }

    public Func<VKList<Group>, ListWithCount<GroupHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<Group>, ListWithCount<GroupHeader>>) (res =>
        {
          ListWithCount<GroupHeader> listWithCount = new ListWithCount<GroupHeader>()
          {
            TotalCount = res.count
          };
          foreach (Group group in res.items)
            listWithCount.List.Add(new GroupHeader(group, (User) null));
          return listWithCount;
        });
      }
    }

    public GroupsSearchDataProvider(IEnumerable<GroupHeader> localItems)
    {
      this.LocalItems = localItems;
    }

    public string GetFooterTextForCount(int count)
    {
      if (count == 0)
        return CommonResources.NoCommunites;
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneCommunityFrm, CommonResources.TwoFourCommunitiesFrm, CommonResources.FiveCommunitiesFrm, true, null, false);
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<Group>, ResultCode>> callback)
    {
      GroupsService.Current.Search(searchString, offset, count, callback);
    }
  }
}
