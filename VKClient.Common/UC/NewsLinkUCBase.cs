using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public abstract class NewsLinkUCBase : UserControlVirtualizable
  {
    public abstract void Initialize(Link link, double width);

    public abstract double CalculateTotalHeight();

    protected double GetElementTotalHeight(FrameworkElement element)
    {
      Thickness margin = element.Margin;
      double num = margin.Top + element.Height;
      margin = element.Margin;
      double bottom = margin.Bottom;
      return num + bottom;
    }
  }
}
