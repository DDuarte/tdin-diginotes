using System;
using System.Collections.Generic;
using Common;
using Remotes;

namespace Server
{
    /// <summary>
    /// Possible transactions:
    /// - New user
    /// - Password renamed
    /// - New purchase order
    /// - New sales order
    /// - Edit purchase order
    /// - Edit sales order
    /// - 
    /// </summary>
    public interface ILogAction
    {
        void Apply(IDigiMarket digiMarket);
    }

    public class NewUserAction : ILogAction
    {
        public string Name { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.Register(Name, User, Password);
        }
    }

    public class QuotationChangeAction : ILogAction
    {
        public decimal Quotation { get; set; }
        public DateTime Time { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.ChangeQuotationDirect(Quotation, Time);
        }
    }

    public class AddFundsAction : ILogAction
    {
        public string User { get; set; }
        public decimal Balance { get; set; }
        public int DiginoteCount { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.AddFundsDirect(User, Balance, DiginoteCount);
        }
    }

    public class OrdersSnapshotAction : ILogAction
    {
        public List<PurchaseOrder> PurchaseOrders { get; set; }
        public List<SalesOrder> SalesOrders { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.OrdersSnapshot(PurchaseOrders, SalesOrders);
        }
    }

    public class UpdateBalanceAction : ILogAction
    {
        public string User { get; set; }
        public decimal Balance { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.SetFundsDirect(User, Balance);
        }
    }

    public class UpdateDiginotesAction : ILogAction
    {
        public string User { get; set; }
        public int Diginotes { get; set; }
        public decimal Value { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.UpdateDiginotesDirect(User, Diginotes, Value);
        }
    }

    public class TransactionAction : ILogAction
    {
        public string Buyer { get; set; }
        public string Seller { get; set; }
        public int Diginotes { get; set; }
        public decimal Cost { get; set; }
        public DateTime Date { get; set; }

        public void Apply(IDigiMarket digiMarket)
        {
            digiMarket.ApplyTransaction(Buyer, Seller, Diginotes, Cost, Date);
        }
    }
}
