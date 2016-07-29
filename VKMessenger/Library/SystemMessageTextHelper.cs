using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Localization;
using VKMessenger.Backend;

namespace VKMessenger.Library
{
  public class SystemMessageTextHelper
  {
    public static string GenerateText(Message message, User user1, User user2, bool extendedText)
    {
      if (message == null)
        return "";
      if (user1 == null)
        user1 = new User();
      if (user2 == null)
        user2 = new User();
      string str1 = SystemMessageTextHelper.CreateUserNameText(user1, false, extendedText);
      string str2 = user2.id > -2000000000L ? SystemMessageTextHelper.CreateUserNameText(user2, true, extendedText) : message.action_email;
      if (!extendedText)
        str1 = "";
      string str3 = "";
      string action = message.action;
      if (!(action == "chat_photo_update"))
      {
        if (!(action == "chat_photo_remove"))
        {
          if (!(action == "chat_create"))
          {
            if (!(action == "chat_title_update"))
            {
              if (!(action == "chat_invite_user"))
              {
                if (action == "chat_kick_user")
                  str3 = message.action_mid != (long) message.uid ? (!user1.IsFemale ? string.Format(CommonResources.ChatKickoutMaleFrm, (object) str1, (object) str2) : string.Format(CommonResources.ChatKickoutFemaleFrm, (object) str1, (object) str2)) : (!user1.IsFemale ? string.Format(CommonResources.ChatLeftConversationMaleFrm, (object) str1) : string.Format(CommonResources.ChatLeftConversationFemaleFrm, (object) str1));
              }
              else
                str3 = message.action_mid != (long) message.uid ? (!user1.IsFemale ? string.Format(CommonResources.ChatInviteMaleFrm, (object) str1, (object) str2) : string.Format(CommonResources.ChatInviteFemaleFrm, (object) str1, (object) str2)) : (!user1.IsFemale ? string.Format(CommonResources.ChatReturnedToConversationMaleFrm, (object) str1) : string.Format(CommonResources.ChatReturnedToConversationFemaleFrm, (object) str1));
            }
            else
              str3 = !user1.IsFemale ? string.Format(CommonResources.ChatTitleUpdateMaleFrm, (object) str1, (object) message.action_text) : string.Format(CommonResources.ChatTitleUpdateFemaleFrm, (object) str1, (object) message.action_text);
          }
          else
            str3 = !user1.IsFemale ? string.Format(CommonResources.ChatCreateMaleFrm, (object) str1, (object) message.action_text) : string.Format(CommonResources.ChatCreateFemaleFrm, (object) str1, (object) message.action_text);
        }
        else
          str3 = !user1.IsFemale ? string.Format(CommonResources.ChatPhotoDeleteMaleFrm, (object) str1) : string.Format(CommonResources.ChatPhotoDeleteFemaleFrm, (object) str1);
      }
      else
        str3 = !user1.IsFemale ? string.Format(CommonResources.ChatPhotoUpdateMaleFrm, (object) str1) : string.Format(CommonResources.ChatPhotoUpdateFemaleFrm, (object) str1);
      return str3.Trim();
    }

    private static string CreateUserNameText(User user, bool isAcc, bool extendedText)
    {
      string str = isAcc ? user.NameAcc : user.Name;
      if (!extendedText)
        return str;
      if (user.id > 0L)
        return string.Format("[id{0}|{1}]", (object) user.id, (object) str);
      return string.Format("[club{0}|{1}]", (object) -user.id, (object) str);
    }
  }
}
