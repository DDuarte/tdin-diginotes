using System;
using System.Collections.Generic;

namespace Common.Models
{
    public class QuotationChange
    {
        public int QuotationChangeId { get; set; }
        public Market Market { get; set; }
        public DateTime Date { get; set; }
        public decimal Quotation { get; set; }
    }

    public class Market
    {
        private static int _lastId = 1;
        public int MarketId { get; set; }

        private decimal _quotation;
        public decimal Quotation
        {
            get
            {
                return _quotation;
            }
            set
            {
                _quotation = value;
                QuotationHistory.Add(new QuotationChange { Date = DateTime.Now, Market = this, Quotation = _quotation });
            }
        }
        public ICollection<QuotationChange> QuotationHistory { get; set; }
        // public virtual List<PurchaseOrder> PurchaseOrders { get; private set; }
        // public virtual List<SalesOrder> SalesOrders { get; private set; }
        // public virtual List<Transaction> Transactions { get; private set; }
        // public virtual List<User> Users { get; private set; }

        public Market()
        {
            MarketId = _lastId++;
            QuotationHistory = new List<QuotationChange>();
            Quotation = 1;
        }

        public Market(int marketId, decimal quotation)
        {
            MarketId = marketId;
            QuotationHistory = new List<QuotationChange>();
            Quotation = quotation;
            // PurchaseOrders = new List<PurchaseOrder>();
            // SalesOrders = new List<SalesOrder>();
            // Transactions = new List<Transaction>();
            // Users = new List<User>();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Market) obj);
        }

        protected bool Equals(Market other)
        {
            return MarketId == other.MarketId;
        }

        public override int GetHashCode()
        {
            return MarketId;
        }
    }
}
