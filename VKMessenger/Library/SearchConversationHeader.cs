using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class SearchConversationHeader : ConversationHeader
  {
    public Visibility IsOnlineSearch
    {
      get
      {
        return this._associatedUsers != null && this._associatedUsers.Count == 1 && (this._associatedUsers.First<User>().online != 0 && this._associatedUsers.First<User>().online_mobile == 0) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Visibility IsOnlineMobileSearch
    {
      get
      {
        return this._associatedUsers != null && this._associatedUsers.Count == 1 && this._associatedUsers.First<User>().online_mobile != 0 ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public SearchConversationHeader(Message message, List<User> associatedUsers)
      : base(message, associatedUsers, 0)
    {
    }

    protected override string FormatTitleForUser(User user)
    {
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsOnlineSearch));
      return string.Format("{0} {1}", (object) user.first_name, (object) user.last_name);
    }
  }
}
