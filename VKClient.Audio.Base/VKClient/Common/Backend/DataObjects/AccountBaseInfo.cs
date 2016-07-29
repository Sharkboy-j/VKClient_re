using System.Collections.Generic;

namespace VKClient.Common.Backend.DataObjects
{
  public class AccountBaseInfo
  {
    public string support_url { get; set; }

    public List<AccountBaseInfoSettingsEntry> settings { get; set; }
  }
}
