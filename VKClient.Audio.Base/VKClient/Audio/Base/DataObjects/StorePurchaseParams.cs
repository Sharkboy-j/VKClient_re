using System;
using System.Text;

namespace VKClient.Audio.Base.DataObjects
{
  public class StorePurchaseParams
  {
      public int ProductId { get; set; }

      public string MerchantProductId { get; set; }

      public Guid MerchantTransactionId { get; set; }

      public string ReceiptBase64 { get; set; }

    public StorePurchaseReferrer? referrer { get; set; }

    public long UserId { get; set; }

    public StorePurchaseParams(int productId)
    {
      this.ProductId = productId;
    }

    public StorePurchaseParams(string merchantProductId, Guid merchantTransactionId, string receipt)
    {
      this.MerchantProductId = merchantProductId;
      this.MerchantTransactionId = merchantTransactionId;
      string str = "";
      if (!string.IsNullOrEmpty(receipt))
        str = Convert.ToBase64String(Encoding.UTF8.GetBytes(receipt));
      this.ReceiptBase64 = str;
    }
  }
}
