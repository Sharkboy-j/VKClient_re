using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Library;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Library;

namespace VKClient.Audio.Base.BackendServices
{
  public class StoreService
  {
    private static StoreService _instance;

    public static StoreService Instance
    {
      get
      {
        return StoreService._instance ?? (StoreService._instance = new StoreService());
      }
    }

    public void BuyProduct(StoreBuyProductParams buyParams, Action<BackendResult<StoreBuyProductResult, ResultCode>> successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      if (buyParams == null || buyParams.ProductType != StoreProductType.stickers)
        return;
      this.FulfillPurchases(null, (Action) (() => this.DoBuyProduct(buyParams, successCallback, errorCallback)), errorCallback);
    }

    private void DoBuyProduct(StoreBuyProductParams buyParams, Action<BackendResult<StoreBuyProductResult, ResultCode>> successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          buyParams.ProductType.ToString()
        },
        {
          "product_id",
          buyParams.ProductId.ToString()
        },
        {
          "guid",
          buyParams.RandomId.ToString()
        },
        {
          "privacy",
          ((int) buyParams.Privacy).ToString()
        }
      };
      if (buyParams.UserIds != null)
        parameters["user_ids"] = string.Join<long>(",", (IEnumerable<long>) buyParams.UserIds);
      if (!string.IsNullOrEmpty(buyParams.Message))
        parameters["message"] = buyParams.Message;
      if (AppGlobalStateManager.Current.GlobalState.PaymentType == AccountPaymentType.money)
        parameters["no_inapp"] = "1";
      if (!string.IsNullOrEmpty(buyParams.StickerReferrer))
        parameters["sticker_referrer"] = buyParams.StickerReferrer;
      VKRequestsDispatcher.DispatchRequestToVK<StoreBuyProductResult>("store.buyProduct", parameters, (Action<BackendResult<StoreBuyProductResult, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          Action<BackendResult<StoreBuyProductResult, ResultCode>> action = successCallback;
          if (action == null)
            return;
          BackendResult<StoreBuyProductResult, ResultCode> backendResult = result;
          action(backendResult);
        }
        else
        {
          Action<BackendResult<StorePurchaseResult, ResultCode>> action = errorCallback;
          if (action == null)
            return;
          BackendResult<StorePurchaseResult, ResultCode> backendResult = new BackendResult<StorePurchaseResult, ResultCode>(result.ResultCode, StorePurchaseResult.GetForFailedPurchaseState());
          action(backendResult);
        }
      }), (Func<string, StoreBuyProductResult>) null, false, true, new CancellationToken?());
    }

    public void PurchaseVotesPack(VotesPack votesPack, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback, Action cancelledCallback)
    {
      if (votesPack.PaymentType == AccountPaymentType.inapp)
        this.FulfillPurchases(votesPack.MerchantProductId, (Action) (() => this.PurchaseVotesPackWithInApps(votesPack, successCallback, errorCallback, cancelledCallback)), errorCallback);
      else
        this.PurchaseVotesPackWithMobile(votesPack, successCallback, errorCallback);
    }

    private void PurchaseVotesPackWithInApps(VotesPack votesPack, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback, Action cancelledCallback)
    {
      InAppPurchaseService.RequestProductPurchase(votesPack.MerchantProductId, (Action<InAppProductPurchaseResult>) (purchaseResult =>
      {
        switch (purchaseResult.Status)
        {
          case InAppProductPurchaseStatus.Cancelled:
            Action action1 = cancelledCallback;
            if (action1 == null)
              break;
            action1();
            break;
          case InAppProductPurchaseStatus.Purchased:
            this.FulfillPurchases(votesPack.MerchantProductId, successCallback, errorCallback);
            break;
          case InAppProductPurchaseStatus.Error:
            Action<BackendResult<StorePurchaseResult, ResultCode>> action2 = errorCallback;
            if (action2 == null)
              break;
            BackendResult<StorePurchaseResult, ResultCode> backendResult = new BackendResult<StorePurchaseResult, ResultCode>(ResultCode.Succeeded, StorePurchaseResult.GetForFailedPurchaseState());
            action2(backendResult);
            break;
        }
      }));
    }

    private void PurchaseVotesPackWithMobile(VotesPack votesPack, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      this.BuyVotes(new StoreBuyProductParams(StoreProductType.votes, votesPack.ProductId), (Action<BackendResult<StorePurchaseResult, ResultCode>>) (result =>
      {
        if (result.ResultCode == ResultCode.Succeeded)
        {
          Action action = successCallback;
          if (action == null)
            return;
          action();
        }
        else
        {
          Action<BackendResult<StorePurchaseResult, ResultCode>> action = errorCallback;
          if (action == null)
            return;
          BackendResult<StorePurchaseResult, ResultCode> backendResult = result;
          action(backendResult);
        }
      }));
    }

    private void BuyVotes(StoreBuyProductParams buyParams, Action<BackendResult<StorePurchaseResult, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<StorePurchaseResult>("store.buyVotes", new Dictionary<string, string>()
      {
        {
          "product_id",
          buyParams.ProductId.ToString()
        },
        {
          "guid",
          buyParams.RandomId.ToString()
        }
      }, callback, (Func<string, StorePurchaseResult>) null, false, true, new CancellationToken?());
    }

    public void RestorePurchases(string productId, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      this.FulfillPurchases(productId, successCallback, errorCallback);
    }

    private void FulfillPurchases(string productId, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      InAppPurchaseService.LoadUnfulfilledConsumables(productId, (Action<List<InAppUnfulfilledProduct>>) (unfulfilledConsumables =>
      {
        if (unfulfilledConsumables != null && unfulfilledConsumables.Count > 0)
        {
          this.DoFulfillPurchases((IList<InAppUnfulfilledProduct>) unfulfilledConsumables, successCallback, errorCallback);
        }
        else
        {
          Action action = successCallback;
          if (action == null)
            return;
          action();
        }
      }));
    }

    private void DoFulfillPurchases(IList<InAppUnfulfilledProduct> products, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      if (products.Count == 0)
      {
        Action action = successCallback;
        if (action == null)
          return;
        action();
      }
      else
      {
        InAppUnfulfilledProduct product = products[0];
        string productId = product.ProductId;
        Guid transactionId = product.TransactionId;
        InAppPurchaseService.LoadProductReceipt(productId, (Action<string>) (receipt => this.Purchase(new StorePurchaseParams(productId, transactionId, receipt), (Action) (() =>
        {
          products.Remove(product);
          InAppPurchaseService.ReportConsumableProductFulfillment(product, (Action) (() => this.DoFulfillPurchases(products, successCallback, errorCallback)));
        }), errorCallback)));
      }
    }

    private void Purchase(StorePurchaseParams purchaseParams, Action successCallback, Action<BackendResult<StorePurchaseResult, ResultCode>> errorCallback)
    {
      if (purchaseParams == null || string.IsNullOrEmpty(purchaseParams.MerchantProductId) && purchaseParams.ProductId == 0)
        return;
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          "votes"
        },
        {
          "merchant",
          "microsoft"
        }
      };
      long num = (long) purchaseParams.ProductId;
      if (num != 0L)
      {
        parameters["product_id"] = num.ToString();
      }
      else
      {
        parameters["merchant_product_id"] = purchaseParams.MerchantProductId;
        parameters["merchant_transaction_id"] = purchaseParams.MerchantTransactionId.ToString();
        parameters["receipt"] = purchaseParams.ReceiptBase64;
      }
      StorePurchaseReferrer? referrer = purchaseParams.referrer;
      if (referrer.HasValue)
        parameters["referrer"] = referrer.Value.ToString();
      long userId = purchaseParams.UserId;
      if (userId > 0L)
        parameters["user_id"] = userId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<StorePurchaseResult>("store.purchase", parameters, (Action<BackendResult<StorePurchaseResult, ResultCode>>) (response =>
      {
        Action action1 = (Action) (async () =>
        {
          await Task.Delay(TimeSpan.FromSeconds(1.0));
          this.Purchase(purchaseParams, successCallback, errorCallback);
        });
        if (response.ResultCode == ResultCode.TooManyRequestsPerSecond)
          action1();
        else if (response.ResultData == null || response.ResultData.state < 0 && response.ResultData.state != -8)
        {
          Action<BackendResult<StorePurchaseResult, ResultCode>> action2 = errorCallback;
          if (action2 == null)
            return;
          BackendResult<StorePurchaseResult, ResultCode> backendResult = response;
          action2(backendResult);
        }
        else if (response.ResultData.state == 0)
        {
          action1();
        }
        else
        {
          Action action2 = successCallback;
          if (action2 == null)
            return;
          action2();
        }
      }), (Func<string, StorePurchaseResult>) null, false, true, new CancellationToken?());
    }

    public void GetStickersStoreCatalog(Action<BackendResult<StoreCatalog, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<StoreCatalog>("execute.getStickersStoreCatalog", new Dictionary<string, string>(), callback, (Func<string, StoreCatalog>) null, false, true, new CancellationToken?());
    }

    public void GetBalanceData(Action<BackendResult<BalanceData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<BalanceDataResponse>("execute.getBalanceData", new Dictionary<string, string>()
      {
        {
          "type",
          "votes"
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        }
      }, (Action<BackendResult<BalanceDataResponse, ResultCode>>) (result =>
      {
        BalanceData balanceData = new BalanceData();
        ResultCode resultCode = result.ResultCode;
        BalanceDataResponse resultData = result.ResultData;
        List<StockItem> stockItems1 = (List<StockItem>) null;
        BackendResult<BalanceData, ResultCode> backendResult1 = new BackendResult<BalanceData, ResultCode>();
        backendResult1.ResultCode = resultCode;
        BalanceData balanceData1 = balanceData;
        backendResult1.ResultData = balanceData1;
        BackendResult<BalanceData, ResultCode> response = backendResult1;
        if (resultData != null)
        {
          balanceData.Balance = resultData.balance;
          VKList<StockItem> stockItems2 = resultData.stockItems;
          stockItems1 = stockItems2 != null ? stockItems2.items : (List<StockItem>) null;
        }
        if (result.ResultCode == ResultCode.Succeeded && stockItems1 != null)
        {
          VotesPacksGenerator.Generate(AppGlobalStateManager.Current.GlobalState.PaymentType, stockItems1, (Action<List<VotesPack>>) (votesPacks =>
          {
            balanceData.VotesPacks = votesPacks;
            Action<BackendResult<BalanceData, ResultCode>> action = callback;
            if (action == null)
              return;
            BackendResult<BalanceData, ResultCode> backendResult2 = response;
            action(backendResult2);
          }));
        }
        else
        {
          Action<BackendResult<BalanceData, ResultCode>> action = callback;
          if (action == null)
            return;
          BackendResult<BalanceData, ResultCode> backendResult2 = response;
          action(backendResult2);
        }
      }), (Func<string, BalanceDataResponse>) null, false, true, new CancellationToken?());
    }

    public void GetStickersKeywords(Action<BackendResult<StickersKeywordsData, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<StickersKeywordsData>("store.getStickersKeywords", new Dictionary<string, string>()
      {
        {
          "aliases",
          "1"
        },
        {
          "all_products",
          "1"
        }
      }, callback, (Func<string, StickersKeywordsData>) null, false, true, new CancellationToken?());
    }

    public void GetStockItemByStickerId(long stickerId, Action<BackendResult<StockItem, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      VKRequestsDispatcher.DispatchRequestToVK<StockItem>("store.getStockItemByStickerId", new Dictionary<string, string>()
      {
        {
          "sticker_id",
          stickerId.ToString()
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        }
      }, callback, (Func<string, StockItem>) null, false, true, cancellationToken);
    }

    public void GetStockItemByName(string name, Action<BackendResult<StockItem, ResultCode>> callback, CancellationToken? cancellationToken = null)
    {
      VKRequestsDispatcher.DispatchRequestToVK<StockItem>("store.getStockItemByName", new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "name",
          name
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        }
      }, callback, (Func<string, StockItem>) null, false, true, cancellationToken);
    }

    public void GetStockItems(StoreProductType productType, List<long> productIds = null, List<StoreProductFilter> productFilters = null, long purchaseForId = 0, Action<BackendResult<VKList<StockItem>, ResultCode>> callback = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          productType.ToString()
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        }
      };
      if (productIds != null && productIds.Count > 0)
        parameters["product_ids"] = string.Join<long>(",", (IEnumerable<long>) productIds);
      if (productFilters != null && productFilters.Count > 0)
        parameters["filters"] = string.Join(",", productFilters.Select<StoreProductFilter, string>((Func<StoreProductFilter, string>) (filter => filter.ToString().ToLowerInvariant())));
      if (purchaseForId > 0L)
        parameters["purchase_for"] = purchaseForId.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<VKList<StockItem>>("store.getStockItems", parameters, callback, (Func<string, VKList<StockItem>>) null, false, true, new CancellationToken?());
    }

    public void GetProducts(List<StoreProductFilter> productFilters = null, Action<BackendResult<VKList<StoreProduct>, ResultCode>> callback = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        },
        {
          "extended",
          "1"
        }
      };
      if (productFilters != null && productFilters.Count > 0)
        parameters["filters"] = string.Join(",", productFilters.Select<StoreProductFilter, string>((Func<StoreProductFilter, string>) (filter => filter.ToString().ToLowerInvariant())));
      VKRequestsDispatcher.DispatchRequestToVK<VKList<StoreProduct>>("store.getProducts", parameters, callback, (Func<string, VKList<StoreProduct>>) null, false, true, new CancellationToken?());
    }

    public void GetProducts(StoreProductFilter productFilter, Action<BackendResult<VKList<StoreProduct>, ResultCode>> callback = null)
    {
      this.GetProducts(new List<StoreProductFilter>()
      {
        productFilter
      }, callback);
    }

    public void GetStockItems(List<StoreProductFilter> productFilters = null, Action<BackendResult<List<StockItem>, ResultCode>> callback = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "merchant",
          "microsoft"
        },
        {
          "no_inapp",
          "1"
        },
        {
          "extended",
          "1"
        }
      };
      if (productFilters != null && productFilters.Count > 0)
        parameters["filters"] = string.Join(",", productFilters.Select<StoreProductFilter, string>((Func<StoreProductFilter, string>) (filter => filter.ToString().ToLowerInvariant())));
      VKRequestsDispatcher.DispatchRequestToVK<ProductsWithStockItems>("execute.getProductsWithStockItems", parameters, (Action<BackendResult<ProductsWithStockItems, ResultCode>>) (result =>
      {
        List<StockItem> resultData1 = new List<StockItem>();
        if (result.ResultCode == ResultCode.Succeeded)
        {
          ProductsWithStockItems resultData2 = result.ResultData;
          if ((resultData2 != null ? resultData2.products.items : (List<StoreProduct>) null) != null)
          {
            VKList<StockItem> stockItems = resultData2.stockItems;
            if ((stockItems != null ? stockItems.items : (List<StockItem>) null) != null)
            {
              foreach (StoreProduct storeProduct in resultData2.products.items)
              {
                StoreProduct product = storeProduct;
                StockItem stockItem = resultData2.stockItems.items.FirstOrDefault<StockItem>((Func<StockItem, bool>) (i => i.product.id == product.id));
                if (stockItem != null)
                  resultData1.Add(stockItem);
              }
            }
          }
        }
        Action<BackendResult<List<StockItem>, ResultCode>> action = callback;
        if (action == null)
          return;
        BackendResult<List<StockItem>, ResultCode> backendResult = new BackendResult<List<StockItem>, ResultCode>(result.ResultCode, resultData1);
        action(backendResult);
      }), (Func<string, ProductsWithStockItems>) null, false, true, new CancellationToken?());
    }

    public void ActivateProduct(int productId, Action<BackendResult<bool, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("store.activateProduct", new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "product_id",
          productId.ToString()
        }
      }, (Action<BackendResult<int, ResultCode>>) (result =>
      {
        bool resultData = result.ResultCode == ResultCode.Succeeded && result.ResultData == 1;
        callback(new BackendResult<bool, ResultCode>(result.ResultCode, resultData));
      }), (Func<string, int>) null, false, true, new CancellationToken?());
    }

    public void DeactivateProduct(int productId, Action<BackendResult<bool, ResultCode>> callback)
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("store.deactivateProduct", new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "product_id",
          productId.ToString()
        }
      }, (Action<BackendResult<int, ResultCode>>) (result =>
      {
        bool resultData = result.ResultCode == ResultCode.Succeeded && result.ResultData == 1;
        callback(new BackendResult<bool, ResultCode>(result.ResultCode, resultData));
      }), (Func<string, int>) null, false, true, new CancellationToken?());
    }

    public void ReorderProducts(int productId, int after = 0, int before = 0, Action<BackendResult<bool, ResultCode>> callback = null)
    {
      Dictionary<string, string> parameters = new Dictionary<string, string>()
      {
        {
          "type",
          "stickers"
        },
        {
          "product_id",
          productId.ToString()
        }
      };
      if (after > 0)
        parameters["after"] = after.ToString();
      if (before > 0)
        parameters["before"] = before.ToString();
      VKRequestsDispatcher.DispatchRequestToVK<int>("store.reorderProducts", parameters, (Action<BackendResult<int, ResultCode>>) (result =>
      {
        bool resultData = result.ResultCode == ResultCode.Succeeded && result.ResultData == 1;
        Action<BackendResult<bool, ResultCode>> action = callback;
        if (action == null)
          return;
        BackendResult<bool, ResultCode> backendResult = new BackendResult<bool, ResultCode>(result.ResultCode, resultData);
        action(backendResult);
      }), (Func<string, int>) null, false, true, new CancellationToken?());
    }

    public void MarkUpdatesAsViewed()
    {
      VKRequestsDispatcher.DispatchRequestToVK<int>("store.markUpdatesAsViewed", new Dictionary<string, string>(), (Action<BackendResult<int, ResultCode>>) (result => {}), (Func<string, int>) null, false, true, new CancellationToken?());
    }
  }
}
