using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Utils;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
    public class ConversationSearchViewModel : ViewModelBase
    {
        public List<string> AllSearchStrings = new List<string>();
        private Dictionary<string, List<SearchConversationHeader>> CachedConversations = new Dictionary<string, List<SearchConversationHeader>>();
        private Dictionary<string, List<ConversationHeader>> CachedMessages = new Dictionary<string, List<ConversationHeader>>();
        private ObservableCollection<SearchConversationHeader> _conversations = new ObservableCollection<SearchConversationHeader>();
        private ObservableCollection<ConversationHeader> _messages = new ObservableCollection<ConversationHeader>();
        private bool _isLoadingConversations;
        private string _localConversationsQuery;
        private string _localMessagesQuery;
        private bool _isLoadingMessages;
        private int _availableMessages;
        private int _loadedMessages;

        public ObservableCollection<SearchConversationHeader> Conversations
        {
            get
            {
                return this._conversations;
            }
            set
            {
                this._conversations = value;
                this.NotifyPropertyChanged<ObservableCollection<SearchConversationHeader>>((System.Linq.Expressions.Expression<Func<ObservableCollection<SearchConversationHeader>>>)(() => this.Conversations));
            }
        }

        public ObservableCollection<ConversationHeader> Messages
        {
            get
            {
                return this._messages;
            }
            set
            {
                this._messages = value;
                this.NotifyPropertyChanged<ObservableCollection<ConversationHeader>>((System.Linq.Expressions.Expression<Func<ObservableCollection<ConversationHeader>>>)(() => this.Messages));
            }
        }

        public void SearchConversations(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                this.Conversations.Clear();
            }
            else
            {
                string index = query.Trim();
                this._localConversationsQuery = index;
                this.UpdateSearchStrings(index);
                this.Conversations.Clear();
                if (this.CachedConversations.ContainsKey(index))
                    this.Conversations = new ObservableCollection<SearchConversationHeader>(this.CachedConversations[index]);
                else
                    this.SearchConversationsOnline(index);
            }
        }

        public void SearchMessages(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                this.Messages.Clear();
            }
            else
            {
                string index = query.Trim();
                this._localMessagesQuery = index;
                this.UpdateSearchStrings(index);
                this.Messages.Clear();
                this._loadedMessages = 0;
                this._availableMessages = 0;
                if (this.CachedMessages.ContainsKey(index))
                    this.Messages = new ObservableCollection<ConversationHeader>(this.CachedMessages[index]);
                else
                    this.SearchMessages(index, 0, null);
            }
        }

        private void UpdateSearchStrings(string query)
        {
            List<string> list = Enumerable.ToList<string>(Enumerable.Distinct<string>(query.Split(' ')));
            List<string> list2 = new List<string>();
            Func<char, bool> arg_42_1 = new Func<char, bool>(c => Transliteration.IsCyrillic(c));
            if (Enumerable.Any<char>(query, arg_42_1))
            {
                IEnumerable<string> arg_69_0 = list;
                Func<string, string> arg_69_1 = new Func<string, string>(l => Transliteration.CyrillicToLatin(l));
                list2 = Enumerable.ToList<string>(Enumerable.Select<string, string>(arg_69_0, arg_69_1));
            }
            else
            {
                IEnumerable<string> arg_96_0 = list;
                Func<string, string> arg_96_1 = new Func<string, string>(l => Transliteration.CyrillicToLatin(l));
                list2 = Enumerable.ToList<string>(Enumerable.Select<string, string>(arg_96_0, arg_96_1));
            }
            this.AllSearchStrings = Enumerable.ToList<string>(Enumerable.Distinct<string>(Enumerable.Concat<string>(list, list2)));
        }

        private void SearchConversationsOnline(string query)
        {
            if (this._isLoadingConversations)
                return;
            this._isLoadingConversations = true;
            this.SetInProgressMain(true, "");
            BackendServices.MessagesService.SearchDialogs(query, (Action<BackendResult<List<object>, ResultCode>>)(res =>
            {
                this.SetInProgressMain(false, "");
                if (string.Compare(this._localConversationsQuery, query) != 0)
                {
                    this._isLoadingConversations = false;
                    this.SearchConversationsOnline(this._localConversationsQuery);
                }
                else if (res.ResultCode == ResultCode.Succeeded)
                {
                    List<SearchConversationHeader> IntermediateStorage = new List<SearchConversationHeader>();
                    foreach (object obj in res.ResultData.Where<object>((Func<object, bool>)(r => r != null)))
                    {
                        if (obj is User)
                        {
                            User user = obj as User;
                            IntermediateStorage.Add(new SearchConversationHeader(new Message()
                            {
                                uid = (int)user.uid
                            }, new List<User>() { user }));
                        }
                        if (obj is Chat)
                        {
                            Chat chat = obj as Chat;
                            List<SearchConversationHeader> conversationHeaderList = IntermediateStorage;
                            Message message = new Message();
                            message.chat_id = (int)chat.chat_id;
                            message.title = chat.title;
                            message.chat_active_str = string.Join<long>(",", (IEnumerable<long>)chat.users);
                            message.photo_200 = chat.photo_200;
                            List<User> list = chat.users.Select<long, User>((Func<long, User>)(c => new User()
                            {
                                uid = (long)(int)c
                            })).ToList<User>();
                            SearchConversationHeader conversationHeader = new SearchConversationHeader(message, list);
                            conversationHeaderList.Add(conversationHeader);
                        }
                    }
                    this.ApplyUserProfiles(IntermediateStorage, query);
                }
                else
                {
                    this._isLoadingConversations = false;
                    Logger.Instance.Error("Failed to search conversations");
                }
            }));
        }

        private void SearchMessages(string query, int count, Action callback = null)
        {
            if (this._isLoadingMessages)
                return;
            this._isLoadingMessages = true;
            this.SetInProgressMain(true, "");
            BackendServices.MessagesService.SearchMessages(query, VKConstants.DefaultSearchMessagesCount, count, (Action<BackendResult<MessageListResponse, ResultCode>>)(res =>
            {
                this.SetInProgressMain(false, "");
                if (string.Compare(this._localMessagesQuery, query) != 0)
                {
                    this._isLoadingMessages = false;
                    this.SearchMessages(this._localMessagesQuery, count, callback);
                }
                else if (res.ResultCode == ResultCode.Succeeded)
                {
                    res.ResultData.Messages.Select<Message, int>((Func<Message, int>)(m => m.uid)).ToList<int>();
                    if (string.Compare(this._localMessagesQuery, query) != 0)
                    {
                        this._isLoadingMessages = false;
                        this.SearchMessages(this._localMessagesQuery, count, callback);
                    }
                    else
                    {
                        this._availableMessages = res.ResultData.TotalCount;
                        this._loadedMessages = this._loadedMessages + VKConstants.DefaultSearchMessagesCount;
                        Deployment.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            foreach (ConversationHeader conversationHeader in ConversationsViewModel.GetConversationHeaders(res.ResultData.DialogHeaders, res.ResultData.Users))
                                this.Messages.Add(conversationHeader);
                            if (callback == null)
                                return;
                            callback();
                        }));
                        this._isLoadingMessages = false;
                    }
                }
                else
                {
                    this._isLoadingMessages = false;
                    Logger.Instance.Error("Failed to search messages");
                }
            }));
        }

        private void ApplyUserProfiles(List<SearchConversationHeader> IntermediateStorage, string query)
        {
            UsersService.Instance.GetUsers(IntermediateStorage.SelectMany<SearchConversationHeader, User>((Func<SearchConversationHeader, IEnumerable<User>>)(d => (IEnumerable<User>)d._associatedUsers)).Select<User, long>((Func<User, long>)(u => u.uid)).Distinct<long>().ToList<long>(), (Action<BackendResult<List<User>, ResultCode>>)(result =>
            {
                if (string.Compare(this._localConversationsQuery, query) != 0)
                {
                    this._isLoadingConversations = false;
                    this.SearchConversationsOnline(this._localConversationsQuery);
                }
                else
                {
                    if (result.ResultCode == ResultCode.Succeeded)
                    {
                        foreach (SearchConversationHeader conversationHeader in IntermediateStorage)
                        {
                            SearchConversationHeader dialog = conversationHeader;
                            IEnumerable<User> source = result.ResultData.Where<User>((Func<User, bool>)(u => dialog._associatedUsers.Select<User, long>((Func<User, long>)(c => c.uid)).Contains<long>(u.uid)));
                            dialog._associatedUsers = source.ToList<User>();
                        }
                        IntermediateStorage.ForEach((Action<SearchConversationHeader>)(i => i.RefreshUIProperties(false)));
                        this.CachedConversations.Add(query, IntermediateStorage);
                        Deployment.Current.Dispatcher.BeginInvoke((Action)(() => this.Conversations = new ObservableCollection<SearchConversationHeader>(IntermediateStorage)));
                    }
                    else
                        Logger.Instance.Error("Failed to get user profiles");
                    this._isLoadingConversations = false;
                }
            }));
        }

        public bool CanLoadMoreMessages()
        {
            return this._loadedMessages < this._availableMessages;
        }

        internal void LoadMoreMessages(string query, Action callback)
        {
            this.SearchMessages(query, this._loadedMessages, callback);
        }
    }
}
