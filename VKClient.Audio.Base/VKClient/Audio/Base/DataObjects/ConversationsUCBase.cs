using System;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Audio.Base.DataObjects
{
  public abstract class ConversationsUCBase : UserControl
  {
    public bool IsShareContentMode { get; set; }

    public event EventHandler<Action> ConversationTap;

    public abstract void SetListHeader(FrameworkElement element);

    public abstract void PrepareForViewIfNeeded();

    protected void OnConversationTap(Action callback)
    {
      if (this.ConversationTap == null)
        return;
      this.ConversationTap((object) this, callback);
    }
  }
}
