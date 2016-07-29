using System;
using System.Windows;

namespace VKClient.Common.UC
{
  public class MenuItemExtended
  {
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Visibility DescriptionVisibility
    {
      get
      {
        return !string.IsNullOrEmpty(this.Description) ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public Action Action { get; set; }
  }
}
