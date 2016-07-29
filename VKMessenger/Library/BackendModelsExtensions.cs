using System;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKClient.Common.Utils;

namespace VKMessenger.Library
{
  public static class BackendModelsExtensions
  {
    public static string GetUserStatusString(this UserStatus userStatus, bool isMale)
    {
      if (userStatus == null || userStatus.time == 0L)
        return "";
      if (userStatus.online == 1L)
        return CommonResources.Conversation_Online;
      DateTime dateTime = Extensions.UnixTimeStampToDateTime((double) userStatus.time, true);
      string str1 = string.Empty;
      DateTime now = DateTime.Now;
      int int32 = Convert.ToInt32(Math.Floor((now - dateTime).TotalMinutes));
      string str2;
      if (int32 > 0 && int32 < 60)
      {
        if (int32 < 2)
        {
          str2 = !isMale ? CommonResources.Conversation_LastSeenAMomentAgoFemale : CommonResources.Conversation_LastSeenAMomentAgoMale;
        }
        else
        {
          int num = int32 % 10;
          str2 = !isMale ? (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoFemaleFrm, (object) int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoFemaleFrm, (object) int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoFemaleFrm, (object) int32)) : (num != 1 || int32 >= 10 && int32 <= 20 ? (num >= 5 || num == 0 || int32 >= 10 && int32 <= 20 ? string.Format(CommonResources.Conversation_LastSeenXFiveMinutesAgoMaleFrm, (object) int32) : string.Format(CommonResources.Conversation_LastSeenXTwoFourMinutesAgoMaleFrm, (object) int32)) : string.Format(CommonResources.Conversation_LastSeenX1MinutesAgoMaleFrm, (object) int32));
        }
      }
      else
        str2 = !(now.Date == dateTime.Date) ? (!(now.AddDays(-1.0).Date == dateTime.Date) ? (now.Year != dateTime.Year ? (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, (object) dateTime.ToString("dd.MM.yyyy"), (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, (object) dateTime.ToString("dd.MM.yyyy"), (object) dateTime.ToString("HH:mm"))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenOnFemaleFrm, (object) dateTime.ToString("dd.MM"), (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenOnMaleFrm, (object) dateTime.ToString("dd.MM"), (object) dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenYesterdayFemaleFrm, (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenYesterdayMaleFrm, (object) dateTime.ToString("HH:mm")))) : (!isMale ? string.Format(CommonResources.Conversation_LastSeenTodayFemaleFrm, (object) dateTime.ToString("HH:mm")) : string.Format(CommonResources.Conversation_LastSeenTodayMaleFrm, (object) dateTime.ToString("HH:mm")));
      return str2;
    }
  }
}
