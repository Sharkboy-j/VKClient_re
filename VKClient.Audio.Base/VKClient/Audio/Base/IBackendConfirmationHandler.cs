using System;

namespace VKClient.Audio.Base
{
  public interface IBackendConfirmationHandler
  {
    void Confim(string confirmationText, Action<bool> callback);
  }
}
