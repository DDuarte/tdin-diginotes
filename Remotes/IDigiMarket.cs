using System;
using System.Collections.Generic;
using Common;

namespace Remotes
{
    public interface IDigiMarket
    {

        #region Events

        event MessageArrivedEvent MessageArrived;

        #endregion

        void PublishMessage(string message);
        RegisterError Register(string name, string username, string password);
        LoginError Login(String username, String password, out User user);
        LogoutError Logout(String username, String password);
        PurchaseResult CreatePurchaseOrder(String username, String password, int quantity);
        bool UpdatePurchaseOrder(string username, string password, int id, decimal value);
        void DeletePurchaseOrder(string username, string password, int id);
        SalesResult CreateSalesOrder(String username, String password, int quantity);
        List<PurchaseOrder> GetPurchaseOrders(String username, String password);
        List<SalesOrder> GetSalesOrders(String username, String password);

        void ApplyingLogs(bool active);
    }
}
