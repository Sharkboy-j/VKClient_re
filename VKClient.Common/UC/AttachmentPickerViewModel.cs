using System.Collections.Generic;
using System.Collections.ObjectModel;
using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class AttachmentPickerViewModel : ViewModelBase
  {
    private readonly ObservableCollection<AttachmentPickerItem> _attachmentTypes;
    private readonly IPhotoPickerPhotosViewModel _pppVM;
    private int _maxCount;

    public ObservableCollection<AttachmentPickerItem> AttachmentTypes
    {
      get
      {
        return this._attachmentTypes;
      }
    }

    public IPhotoPickerPhotosViewModel PPPVM
    {
      get
      {
        return this._pppVM;
      }
    }

    public int MaxCount
    {
      get
      {
        return this._maxCount;
      }
    }

    public AttachmentPickerViewModel(List<AttachmentPickerItem> attachmentTypes, int maxCount)
    {
      this._attachmentTypes = new ObservableCollection<AttachmentPickerItem>(attachmentTypes);
      this._maxCount = maxCount;
      this._pppVM = Navigator.Current.GetPhotoPickerPhotosViewModelInstance(maxCount);
      this._pppVM.CountToLoad = 5;
    }
  }
}
