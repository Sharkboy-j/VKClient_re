using System;
using System.Collections;
using System.Collections.Generic;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Extensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using Windows.ApplicationModel.Store;

namespace VKClient.Audio.Base.Library
{
  public static class InAppPurchaseService
  {
    private static Dictionary<string, string> _votesPrices;
    private const string PRODUCT_ID_PREFIX = "windows.phone.votes.";

    public static async void LoadUnfulfilledConsumables(string productId, Action<List<InAppUnfulfilledProduct>> callback)
    {
      if (callback == null)
        return;
      List<InAppUnfulfilledProduct> unfulfilledProducts = new List<InAppUnfulfilledProduct>();
      if (AppGlobalStateManager.Current.GlobalState.PaymentType == AccountPaymentType.money)
      {
        callback(unfulfilledProducts);
      }
      else
      {
        try
        {
          using (IEnumerator<UnfulfilledConsumable> enumerator = ((IEnumerable<UnfulfilledConsumable>) await CurrentApp.GetUnfulfilledConsumablesAsync()).GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              UnfulfilledConsumable current = enumerator.Current;
              string merchantProductId = InAppPurchaseService.ToServerMerchantProductId(current.ProductId);
              if (string.IsNullOrEmpty(productId) || !(merchantProductId != productId))
              {
                List<InAppUnfulfilledProduct> unfulfilledProductList = unfulfilledProducts;
                InAppUnfulfilledProduct unfulfilledProduct = new InAppUnfulfilledProduct();
                unfulfilledProduct.ProductId = merchantProductId;
                Guid transactionId = current.TransactionId;
                unfulfilledProduct.TransactionId = transactionId;
                unfulfilledProductList.Add(unfulfilledProduct);
              }
            }
          }
        }
        catch
        {
        }
        callback(unfulfilledProducts);
      }
    }

    public static void ReportConsumableProductFulfillment(InAppUnfulfilledProduct product, Action callback = null)
    {
      if (product == null)
        return;
      Execute.ExecuteOnUIThread((Action) (async () =>
      {
        try
        {
          FulfillmentResult fulfillmentResult = await CurrentApp.ReportConsumableFulfillmentAsync(InAppPurchaseService.ToInAppProductId(product.ProductId), product.TransactionId);
        }
        catch
        {
        }
        Action action = callback;
        if (action == null)
          return;
        action();
      }));
    }

    public static async void LoadProductReceipt(string productId, Action<string> callback)
    {
      if (callback == null)
        return;
      string receipt = "";
      try
      {
        productId = InAppPurchaseService.ToInAppProductId(productId);
        receipt = await CurrentApp.GetProductReceiptAsync(productId);
      }
      catch
      {
      }
      callback(receipt);
    }

    public static void RequestProductPurchase(string productId, Action<InAppProductPurchaseResult> callback)
    {
      if (callback == null)
        return;
      Execute.ExecuteOnUIThread((Action) (async () =>
      {
        InAppProductPurchaseResult purchaseResult = new InAppProductPurchaseResult()
        {
          Status = InAppProductPurchaseStatus.Cancelled
        };
        try
        {
          productId = InAppPurchaseService.ToInAppProductId(productId);
          PurchaseResults purchaseResults = await CurrentApp.RequestProductPurchaseAsync(productId);
          if (purchaseResults != null)
          {
            purchaseResult.ReceiptXml = purchaseResults.ReceiptXml;
            purchaseResult.TransactionId = purchaseResults.TransactionId;
            if (!string.IsNullOrEmpty(purchaseResults.ReceiptXml))
              purchaseResult.Status = InAppProductPurchaseStatus.Purchased;
          }
        }
        catch
        {
          purchaseResult.Status = InAppProductPurchaseStatus.Error;
        }
        callback(purchaseResult);
      }));
    }

    public static async void LoadProductPrices(Action<Dictionary<string, string>> callback)
    {
      //int num;
      if (/*num != 0 &&*/ InAppPurchaseService._votesPrices != null)
      {
        Action<Dictionary<string, string>> action = callback;
        if (action == null)
          return;
        Dictionary<string, string> dictionary = InAppPurchaseService._votesPrices;
        action(dictionary);
      }
      else
      {
        try
        {
          InAppPurchaseService._votesPrices = new Dictionary<string, string>();
          ListingInformation listingInformation = await CurrentApp.LoadListingInformationAsync();
          using (IEnumerator<ProductListing> enumerator = listingInformation.ProductListings.Values.GetEnumerator())
          {
            while (((IEnumerator) enumerator).MoveNext())
            {
              ProductListing current = enumerator.Current;
              string index = string.Format("windows.phone.votes.{0}", (object) current.ProductId.ToLowerInvariant());
              InAppPurchaseService._votesPrices[index] = listingInformation.ProductListings[current.ProductId].FormattedPrice;
            }
          }
        }
        catch
        {
        }
        Action<Dictionary<string, string>> action = callback;
        if (action == null)
          return;
        Dictionary<string, string> dictionary = InAppPurchaseService._votesPrices;
        action(dictionary);
      }
    }

    private static string ToInAppProductId(string productId)
    {
      return productId.Substring("windows.phone.votes.".Length).Capitalize();
    }

    private static string ToServerMerchantProductId(string productId)
    {
      return string.Format("{0}{1}", (object) "windows.phone.votes.", (object) productId.ToLowerInvariant());
    }
  }
}
