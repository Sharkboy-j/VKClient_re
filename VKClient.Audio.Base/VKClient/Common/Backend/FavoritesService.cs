using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend.DataObjects;

namespace VKClient.Common.Backend
{
  public class FavoritesService
  {
    private static FavoritesService _instance;

    public static FavoritesService Instance
    {
      get
      {
        if (FavoritesService._instance == null)
          FavoritesService._instance = new FavoritesService();
        return FavoritesService._instance;
      }
    }

    public void GetFavePhotos(int offset, int count, Action<BackendResult<PhotosListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<PhotosListWithCount>("fave.getPhotos", parameters, callback, (Func<string, PhotosListWithCount>) (jsonStr =>
      {
        GenericRoot<VKList<Photo>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<VKList<Photo>>>(jsonStr);
        return new PhotosListWithCount()
        {
          response = genericRoot.response.items,
          photosCount = genericRoot.response.count
        };
      }), false, true, new CancellationToken?());
    }

    public void GetFaveVideos(int offset, int count, Action<BackendResult<VKList<VKClient.Common.Backend.DataObjects.Video>, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["count"] = count.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<VKList<VKClient.Common.Backend.DataObjects.Video>>("fave.getVideos", parameters, callback, (Func<string, VKList<VKClient.Common.Backend.DataObjects.Video>>) null, false, true, new CancellationToken?());
    }

    public void GetFavePosts(int offset, int count, Action<BackendResult<WallData, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["offset"] = offset.ToString();
      parameters["extended"] = "1";
      VKRequestsDispatcher.DispatchRequestToVK<WallData>("fave.getPosts", parameters, callback, (Func<string, WallData>) (jsonStr =>
      {
        GenericRoot<WallDataResponse> genericRoot = JsonConvert.DeserializeObject<GenericRoot<WallDataResponse>>(jsonStr);
        return new WallData()
        {
          groups = genericRoot.response.groups,
          profiles = genericRoot.response.profiles,
          wall = genericRoot.response.items,
          TotalCount = genericRoot.response.count
        };
      }), false, true, new CancellationToken?());
    }

    public void GetFaveUsers(int offset, int count, Action<BackendResult<UsersListWithCount, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      string str = string.Format("  var us = API.fave.getUsers({{\"offset\":{0}, \"count\":{1}}});\r\nvar users = API.users.get({{user_ids: us.items@.id, \"fields\": \"online, online_mobile, photo_max\"}});\r\nif (users)\r\n{{\r\n\r\nreturn users;\r\n}}\r\nreturn [];", (object) offset, (object) count);
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<UsersListWithCount>("execute", parameters, callback, (Func<string, UsersListWithCount>) (jsonStr =>
      {
        GenericRoot<List<User>> genericRoot = JsonConvert.DeserializeObject<GenericRoot<List<User>>>(jsonStr);
        return new UsersListWithCount()
        {
          users = genericRoot.response
        };
      }), false, true, new CancellationToken?());
    }

    public void GetFaveLinks(Action<BackendResult<List<Link>, ResultCode>> callback)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      string methodName = "fave.getLinks";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<List<Link>, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.DispatchRequestToVK<List<Link>>(methodName, parameters, callback1, (Func<string, List<Link>>) (jsonStr => JsonConvert.DeserializeObject<GenericRoot<VKList<Link>>>(jsonStr).response.items), num1 != 0, num2 != 0, cancellationToken);
    }

    public void GetFaveProducts(int offset, int count, Action<BackendResult<VKList<Product>, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<VKList<Product>>("fave.getMarketItems", new Dictionary<string, string>()
      {
        {
          "offset",
          offset.ToString()
        },
        {
          "count",
          count.ToString()
        },
        {
          "extended",
          "1"
        }
      }, callback, (Func<string, VKList<Product>>) null, false, true, new CancellationToken?());
    }

    public void FaveAddRemoveUser(long userId, bool add, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["user_id"] = userId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(add ? "fave.addUser" : "fave.removeUser", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    public void FaveAddRemoveGroup(long groupId, bool add, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["group_id"] = groupId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>(add ? "fave.addGroup" : "fave.removeGroup", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }
  }
}
