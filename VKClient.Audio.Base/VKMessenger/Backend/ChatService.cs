using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;

namespace VKMessenger.Backend
{
  public class ChatService : IChatService
  {
    public void GetChatInfo(long chatId, Action<BackendResult<ChatInfo, ResultCode>> callback)
    {
      string str = string.Format("return {{\"chat\" : API.messages.getChat({{\"chat_id\": {0} }}),\r\n\"chat_participants\" : API.messages.getChatUsers({{\"chat_id\": {0} , \"fields\": \"uid,first_name,last_name,first_name_acc,last_name_acc,online,online_mobile,photo_max,photo_200\" }})}};", (object) chatId);
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      dictionary["code"] = str;
      string methodName = "execute";
      Dictionary<string, string> parameters = dictionary;
      Action<BackendResult<ChatInfo, ResultCode>> callback1 = callback;
      int num1 = 0;
      int num2 = 1;
      CancellationToken? cancellationToken = new CancellationToken?();
      VKRequestsDispatcher.DispatchRequestToVK<ChatInfo>(methodName, parameters, callback1, (Func<string, ChatInfo>) (jsonStr => JsonConvert.DeserializeObject<VKRequestsDispatcher.GenericRoot<ChatInfo>>(jsonStr).response), num1 != 0, num2 != 0, cancellationToken);
    }

    public void EditChat(long chatId, string title, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["chat_id"] = chatId.ToString();
      parameters["title"] = title;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.editChat", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    public void RemoveChatUsers(long chatId, List<long> usersToBeRemoved, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      string format = "API.messages.removeChatUser({{\"chat_id\":{0}, \"user_id\":{1} }});";
      string str = "";
      foreach (long num in usersToBeRemoved)
        str = str + string.Format(format, (object) chatId, (object) num) + Environment.NewLine;
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("execute", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    private void RemoveChatUsersOneByOne(long chatId, List<long> usersToBeRemoved, int ind, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      if (ind >= usersToBeRemoved.Count)
        return;
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["chat_id"] = chatId.ToString();
      parameters["user_id"] = usersToBeRemoved[ind].ToString();
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("messages.removeChatUser", parameters, (Action<BackendResult<ResponseWithId, ResultCode>>) (res =>
      {
        if (res.ResultCode == ResultCode.Succeeded && ind + 1 < usersToBeRemoved.Count)
          this.RemoveChatUsersOneByOne(chatId, usersToBeRemoved, ind + 1, callback);
        else
          callback(res);
      }), (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }

    public void AddChatUsers(long chatId, List<long> uids, Action<BackendResult<ResponseWithId, ResultCode>> callback)
    {
      string format = "API.messages.addChatUser({{\"chat_id\":{0}, \"user_id\":{1} }});";
      string str = "";
      foreach (long uid in uids)
        str = str + string.Format(format, (object) chatId, (object) uid) + Environment.NewLine;
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters["code"] = str;
      VKRequestsDispatcher.DispatchRequestToVK<ResponseWithId>("execute", parameters, callback, (Func<string, ResponseWithId>) (jsonStr => new ResponseWithId()), false, true, new CancellationToken?());
    }
  }
}
