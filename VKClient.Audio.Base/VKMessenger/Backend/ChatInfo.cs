using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;

namespace VKMessenger.Backend
{
  public class ChatInfo
  {
    public Chat chat { get; set; }

    public List<ChatUser> chat_participants { get; set; }
  }
}
