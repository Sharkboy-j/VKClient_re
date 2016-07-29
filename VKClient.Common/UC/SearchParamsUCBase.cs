using System.Collections.Generic;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public abstract class SearchParamsUCBase : UserControl
  {
    public abstract Dictionary<string, string> GetParameters();

    public abstract void Initialize(Dictionary<string, string> parameters);
  }
}
