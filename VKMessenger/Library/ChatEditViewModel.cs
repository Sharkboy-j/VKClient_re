using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class ChatEditViewModel : ViewModelBase
  {
    private string _chatName = string.Empty;
    private string _initialChatName = string.Empty;
    private ObservableCollection<ChatParticipant> _chatParticipants = new ObservableCollection<ChatParticipant>();
    private long _chatId;
    private ChatInfo _chatInfo;
    private bool _addingChatUsers;
    private bool _updatingChatPhoto;

    public string Title
    {
      get
      {
        string str = "";
        if (this._chatParticipants.Count > 0)
          str = UIStringFormatterHelper.FormatNumberOfSomething(this._chatParticipants.Count, CommonResources.ChatEdit_OneParticipant, CommonResources.ChatEdit_TwoFourParticipantsFrm, CommonResources.ChatEdit_FiveMoreParticipantsFrm, true, null, false);
        return str.ToUpperInvariant();
      }
    }

    public string ChatName
    {
      get
      {
        return this._chatName;
      }
      set
      {
        this._chatName = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ChatName));
      }
    }

    public ObservableCollection<ChatParticipant> ChatParticipants
    {
      get
      {
        return this._chatParticipants;
      }
    }

    public Visibility DeletePhotoVisibility
    {
      get
      {
        return this._chatInfo == null || string.IsNullOrWhiteSpace(this._chatInfo.chat.photo_200) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string ChatImage
    {
      get
      {
        if (this._chatInfo == null)
          return "";
        if (string.IsNullOrWhiteSpace(this._chatInfo.chat.photo_200))
          return "http://noimage.gif";
        return this._chatInfo.chat.photo_200;
      }
    }

    public ChatEditViewModel(long chatId)
    {
      this._chatId = chatId;
    }

    public void LoadChatInfoAsync()
    {
      BackendServices.ChatService.GetChatInfo(this._chatId, (Action<BackendResult<ChatInfo, ResultCode>>) (result =>
      {
        if (result.ResultCode != ResultCode.Succeeded)
          return;
        this._chatInfo = result.ResultData;
        Deployment.Current.Dispatcher.BeginInvoke((Action) (() =>
        {
          this.ChatName = result.ResultData.chat.title;
          this._initialChatName = this.ChatName;
          this._chatParticipants.Clear();
          foreach (ChatUser chatParticipant in result.ResultData.chat_participants)
            this._chatParticipants.Add(new ChatParticipant(chatParticipant));
          this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
          this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.ChatImage));
          this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.DeletePhotoVisibility));
        }));
      }));
    }

    public void SaveChat()
    {
      BackendServices.ChatService.EditChat(this._chatId, this.ChatName, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}));
      this._initialChatName = this.ChatName;
      this._chatInfo.chat.title = this.ChatName;
      ConversationHeader conversationHeader = ConversationsViewModel.Instance.Conversations.FirstOrDefault<ConversationHeader>((Func<ConversationHeader, bool>) (c => c.UserOrChatId == this._chatId));
      if (conversationHeader == null)
        return;
      conversationHeader.UITitle = this.ChatName;
    }

    public List<ChatParticipant> ExcludeMembers(List<ChatParticipant> membersToBeExcluded)
    {
      List<ChatParticipant> chatParticipantList = new List<ChatParticipant>();
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      long result = 0;
      long.TryParse(this._chatInfo.chat.admin_id, out result);
      foreach (ChatParticipant chatParticipant in membersToBeExcluded)
      {
        if (loggedInUserId != result && chatParticipant.InvitedBy != loggedInUserId && (long) chatParticipant.ChatUser.uid != loggedInUserId)
          chatParticipantList.Add(chatParticipant);
      }
      foreach (ChatParticipant chatParticipant in chatParticipantList)
        membersToBeExcluded.Remove(chatParticipant);
      if (membersToBeExcluded.Count > 0)
        BackendServices.ChatService.RemoveChatUsers(this._chatId, membersToBeExcluded.Select<ChatParticipant, long>((Func<ChatParticipant, long>) (m => (long) m.ChatUser.uid)).ToList<long>(), (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}));
      foreach (ChatParticipant chatParticipant in membersToBeExcluded)
        this._chatParticipants.Remove(chatParticipant);
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
      return chatParticipantList;
    }

    public bool CanSaveChat()
    {
      if (!string.IsNullOrWhiteSpace(this.ChatName))
        return this.ChatName != this._initialChatName;
      return false;
    }

    internal void LeaveChat()
    {
      BackendServices.ChatService.RemoveChatUsers(this._chatId, new List<long>()
      {
        AppGlobalStateManager.Current.LoggedInUserId
      }, (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => {}));
    }

    internal void AddUsersToChat(List<User> selectedUsers)
    {
      if (selectedUsers == null)
        return;
      foreach (ChatParticipant chatParticipant in (Collection<ChatParticipant>) this._chatParticipants)
      {
        ChatParticipant chatPart = chatParticipant;
        User user = selectedUsers.FirstOrDefault<User>((Func<User, bool>) (u => u.uid == (long) chatPart.ChatUser.uid));
        if (user != null)
          selectedUsers.Remove(user);
      }
      if (selectedUsers.Count == 0 || this._addingChatUsers)
        return;
      this._addingChatUsers = true;
      this.SetInProgress(true, "");
      BackendServices.ChatService.AddChatUsers(this._chatId, selectedUsers.Select<User, long>((Func<User, long>) (u => u.uid)).ToList<long>(), (Action<BackendResult<VKClient.Common.Backend.DataObjects.ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
      {
        this._addingChatUsers = false;
        this.SetInProgress(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
        {
          foreach (User selectedUser in selectedUsers)
            this._chatParticipants.Add(new ChatParticipant(new ChatUser()
            {
              uid = (int) selectedUser.uid,
              photo_rec = selectedUser.photo_rec,
              photo_max = selectedUser.photo_max,
              first_name = selectedUser.first_name,
              last_name = selectedUser.last_name,
              online = selectedUser.online,
              type = "profile",
              invited_by = (int) AppGlobalStateManager.Current.LoggedInUserId
            }));
          this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Title));
        }
        else
          ExtendedMessageBox.ShowSafe(CommonResources.Error);
      }))));
    }

    internal void UpdateChatPhoto(Stream photoStream, Rect rect)
    {
      if (this._updatingChatPhoto)
        return;
      this._updatingChatPhoto = true;
      this.SetInProgress(true, "");
      ImagePreprocessor.PreprocessImage(photoStream, VKConstants.ResizedImageSize, true, (Action<ImagePreprocessResult>) (pres =>
      {
        Stream stream = pres.Stream;
        byte[] photoData = ImagePreprocessor.ReadFully(stream);
        stream.Close();
        BackendServices.MessagesService.UpdateChatPhoto(this._chatId, photoData, ImagePreprocessor.GetThumbnailRect((double) pres.Width, (double) pres.Height, rect), new Action<BackendResult<ChatInfoWithMessageId, ResultCode>>(this.ProcessUpdateDeleteResult));
      }));
    }

    internal void DeleteChatPhoto()
    {
      if (this._updatingChatPhoto)
        return;
      this._updatingChatPhoto = true;
      this.SetInProgress(true, "");
      BackendServices.MessagesService.DeleteChatPhoto(this._chatId, new Action<BackendResult<ChatInfoWithMessageId, ResultCode>>(this.ProcessUpdateDeleteResult));
    }

    private void ProcessUpdateDeleteResult(BackendResult<ChatInfoWithMessageId, ResultCode> res)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        this.SetInProgress(false, "");
        if (res.ResultCode == ResultCode.Succeeded)
          this.LoadChatInfoAsync();
        else
          new GenericInfoUC().ShowAndHideLater(CommonResources.Error, null);
        this._updatingChatPhoto = false;
      }));
    }
  }
}
