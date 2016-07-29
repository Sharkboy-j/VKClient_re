namespace VKClient.Common.Backend
{
  public class GenericRoot<T> where T : class
  {
    public T response { get; set; }
  }
}
