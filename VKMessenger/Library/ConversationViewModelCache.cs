using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class ConversationViewModelCache
  {
    private object _lockObj = new object();
    private Dictionary<string, ConversationViewModel> _inMemoryCachedData = new Dictionary<string, ConversationViewModel>();
    private int _maxNumberOfInMemoryItems = 12;
    private static ConversationViewModelCache _current;

    public static ConversationViewModelCache Current
    {
      get
      {
        if (ConversationViewModelCache._current == null)
          ConversationViewModelCache._current = new ConversationViewModelCache();
        return ConversationViewModelCache._current;
      }
    }

    public void SubscribeToUpdates()
    {
      InstantUpdatesManager.Current.ReceivedUpdates += new InstantUpdatesManager.UpdatesReceivedEventHandler(this.ReceivedUpdates);
    }

    private void ReceivedUpdates(List<LongPollServerUpdateData> updates)
    {
      List<ConversationViewModel> source = new List<ConversationViewModel>();
      foreach (LongPollServerUpdateData update in updates)
      {
        bool isChat = update.isChat;
        long userOrCharId = isChat ? update.chat_id : update.user_id;
        if (userOrCharId != 0L)
        {
          bool onlyInMemoryCache = update.UpdateType != LongPollServerUpdateType.MessageHasBeenAdded;
          ConversationViewModel vm = this.GetVM(userOrCharId, isChat, onlyInMemoryCache);
          if (vm != null)
            source.Add(vm);
        }
      }
      foreach (ConversationViewModel conversationViewModel in source.Distinct<ConversationViewModel>())
        conversationViewModel.ProcessInstantUpdates(updates);
    }

    public void ClearInMemoryCacheImmediately()
    {
      this._inMemoryCachedData.Clear();
    }

    public void FlushToPersistentStorage()
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      Dictionary<string, ConversationViewModel> dictionary = new Dictionary<string, ConversationViewModel>();
      lock (this._lockObj)
      {
        foreach (KeyValuePair<string, ConversationViewModel> item_0 in this._inMemoryCachedData)
          dictionary[item_0.Key] = item_0.Value;
        this._inMemoryCachedData.Clear();
      }
      foreach (KeyValuePair<string, ConversationViewModel> keyValuePair in dictionary)
        CacheManager.TrySerialize((IBinarySerializable) keyValuePair.Value, keyValuePair.Key, true, CacheManager.DataType.CachedData);
      stopwatch.Stop();
      Logger.Instance.Info("ConversationVMCach.SaveToPersistentStorage saved {0} viewmodels in {1} ms.", (object) dictionary.Count, (object) stopwatch.ElapsedMilliseconds);
    }

    public ConversationViewModel GetVM(long userOrCharId, bool isChatId, bool onlyInMemoryCache = false)
    {
      lock (this._lockObj)
      {
        string local_2 = ConversationViewModelCache.GetKey(userOrCharId, isChatId);
        if (this._inMemoryCachedData.ContainsKey(local_2))
          return this._inMemoryCachedData[local_2];
        if (onlyInMemoryCache)
          return (ConversationViewModel) null;
        ConversationViewModel local_3 = new ConversationViewModel();
        if (!CacheManager.TryDeserialize((IBinarySerializable) local_3, local_2, CacheManager.DataType.CachedData))
          local_3.InitializeWith(userOrCharId, isChatId);
        if (local_3.OutboundMessageVm == null || local_3.Messages == null)
        {
          local_3 = new ConversationViewModel();
          local_3.InitializeWith(userOrCharId, isChatId);
        }
        this.SetVM(local_3, false);
        return local_3;
      }
    }

    public void SetVM(ConversationViewModel conversationVM, bool allowFlush)
    {
      if (conversationVM == null)
        return;
      lock (this._lockObj)
      {
        this._inMemoryCachedData[ConversationViewModelCache.GetKey(conversationVM.UserOrCharId, conversationVM.IsChat)] = conversationVM;
        if (!(this._inMemoryCachedData.Count > this._maxNumberOfInMemoryItems & allowFlush))
          return;
        this.FlushToPersistentStorage();
      }
    }

    private static string GetKey(ConversationViewModel cvm)
    {
      return ConversationViewModelCache.GetKey(cvm.UserOrCharId, cvm.IsChat);
    }

    private static string GetKey(long userOrChatId, bool isChatId)
    {
      return "MSG" + AppGlobalStateManager.Current.LoggedInUserId.ToString() + "_" + userOrChatId.ToString() + isChatId.ToString();
    }
  }
}
