using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using VKClient.Common.Framework;

namespace VKMessenger.Library.VirtItems
{
  public class ConversationItems : ViewModelBase
  {
    private ObservableCollection<IVirtualizable> _messages;
    private ConversationViewModel _cvm;
    private ObservableCollection<MessageViewModel> _messagesList;

    public ObservableCollection<IVirtualizable> Messages
    {
      get
      {
        return this._messages;
      }
      set
      {
        this._messages = value;
        this.NotifyPropertyChanged<ObservableCollection<IVirtualizable>>((Expression<Func<ObservableCollection<IVirtualizable>>>) (() => this.Messages));
      }
    }

    public ConversationItems(ConversationViewModel cvm)
    {
      this._cvm = cvm;
      this._cvm.PropertyChanged += new PropertyChangedEventHandler(this._cvm_PropertyChanged);
      this.Messages = new ObservableCollection<IVirtualizable>();
      this.Initialize();
    }

    private void _cvm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "Messages"))
        return;
      this.Initialize();
    }

    internal void Initialize()
    {
      if (this._messagesList != null)
        this._messagesList.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Messages_CollectionChanged);
      this._messagesList = this._cvm.Messages;
      this._messagesList.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Messages_CollectionChanged);
      this._messages.Clear();
      foreach (MessageViewModel mvm in this._messagesList.Reverse<MessageViewModel>())
        this._messages.Add((IVirtualizable) new MessageItem(mvm, this._cvm.Scroll != null && this._cvm.Scroll.IsHorizontalOrientation));
    }

    private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      List<IVirtualizable> virtualizableList = new List<IVirtualizable>();
      if (e.NewItems != null)
      {
        foreach (object newItem in (IEnumerable) e.NewItems)
        {
          if (newItem is MessageViewModel)
          {
            MessageItem messageItem = new MessageItem(newItem as MessageViewModel, this._cvm.Scroll != null && this._cvm.Scroll.IsHorizontalOrientation);
            virtualizableList.Add((IVirtualizable) messageItem);
          }
        }
      }
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        int index = this._messages.Count - e.NewStartingIndex;
        if (index < 0 || index > this._messages.Count)
          return;
        foreach (IVirtualizable virtualizable in virtualizableList)
          this._messages.Insert(index, virtualizable);
      }
      else if (e.Action == NotifyCollectionChangedAction.Reset)
      {
        this._messages.Clear();
      }
      else
      {
        if (e.Action != NotifyCollectionChangedAction.Remove || e.OldItems.Count <= 0)
          return;
        int index = this._messages.Count - e.OldStartingIndex - 1;
        if (index < 0 || index >= this._messages.Count)
          return;
        this._messages.RemoveAt(index);
      }
    }

    internal void Cleanup()
    {
      this._cvm.PropertyChanged -= new PropertyChangedEventHandler(this._cvm_PropertyChanged);
    }
  }
}
