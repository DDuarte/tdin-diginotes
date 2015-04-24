using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class PurchaseOrder
    {
        private static int _lastId = 1;
        public int PurchaseOrderId { get; private set; }
        public int Count { get; private set; }
        public User Buyer { get; private set; }
        public decimal Value { get; private set; }
        public bool Suspended { get; private set; }
        public DateTime Date { get; private set; }

        public PurchaseOrder(User buyer, int count, decimal currentQuotation)
        {
            PurchaseOrderId = _lastId++;
            Buyer = buyer;
            Count = count;
            Value = currentQuotation*count;
            Suspended = false;
            Date = DateTime.Now;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PurchaseOrder) obj);
        }

        protected bool Equals(PurchaseOrder other)
        {
            return PurchaseOrderId == other.PurchaseOrderId;
        }

        public override int GetHashCode()
        {
            return PurchaseOrderId;
        }
    }
}
