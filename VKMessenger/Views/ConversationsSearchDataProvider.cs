using System;
using System.Collections.Generic;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKMessenger.Views
{
  public class ConversationsSearchDataProvider : ISearchDataProvider<User, FriendHeader>
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
        return "";
      }
    }

    public IEnumerable<FriendHeader> LocalItems
    {
      get
      {
        return (IEnumerable<FriendHeader>) null;
      }
    }

    public Func<VKList<User>, ListWithCount<FriendHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<User>, ListWithCount<FriendHeader>>) (res =>
        {
          ListWithCount<FriendHeader> listWithCount = new ListWithCount<FriendHeader>()
          {
            TotalCount = res.count
          };
          foreach (User user in res.items)
            listWithCount.List.Add(new FriendHeader(user, false));
          return listWithCount;
        });
      }
    }

    public string GetFooterTextForCount(int count)
    {
      return "";
    }

    public void GetData(string searchString, Dictionary<string, string> parameters, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      MessagesService.Instance.SearchDialogs(searchString, (Action<BackendResult<List<object>, ResultCode>>) (res =>
      {
        if (res.ResultCode != ResultCode.Succeeded)
        {
          callback(new BackendResult<VKList<User>, ResultCode>(res.ResultCode, (VKList<User>) null));
        }
        else
        {
          List<User> userList = new List<User>();
          foreach (object obj in res.ResultData)
          {
            if (obj is User)
            {
              User user = obj as User;
              userList.Add(user);
            }
            else if (obj is Chat)
            {
              Chat chat = obj as Chat;
              User user = new User()
              {
                first_name = chat.title,
                photo_max = chat.photo_200,
                last_name = "",
                uid = -chat.chat_id
              };
              userList.Add(user);
            }
          }
          callback(new BackendResult<VKList<User>, ResultCode>(res.ResultCode, new VKList<User>()
          {
            count = res.ResultData.Count,
            items = userList
          }));
        }
      }));
    }
  }
}
