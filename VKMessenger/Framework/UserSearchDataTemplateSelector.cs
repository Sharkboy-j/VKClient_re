using System;
using System.Windows.Controls;

namespace VKMessenger.Framework
{
  public class UserSearchDataTemplateSelector : ContentControl
  {
    public event EventHandler<ContentEventArgs> ContentChanged;

    protected override void OnContentChanged(object oldContent, object newContent)
    {
      base.OnContentChanged(oldContent, newContent);
      if (this.ContentChanged == null)
        return;
      this.ContentChanged((object) this, new ContentEventArgs(newContent));
    }
  }
}
