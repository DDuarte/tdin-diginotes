using System;
using System.Collections.Generic;
using Common;

namespace Remotes
{
    public interface IDigiMarket
    {
        RegisterError Register(String username, String password);
        LoginError Login(String username, String password);
        LogoutError Logout(String username, String password);
        PurchaseResult CreatePurchaseOrder(String username, String password, int quantity);
        SalesResult CreateSalesOrder(String username, String password, int quantity);
        List<PurchaseOrder> GetPurchaseOrders(String username, String password);
        List<SalesOrder> GetSalesOrders(String username, String password);

        void ApplyingLogs(bool active);
    }
}
