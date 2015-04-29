using System;

namespace Common
{
    [Serializable]
    public class PurchaseOrder
    {
        private static int _lastId = 1;
        public int Id { get; private set; }
        public int Count { get; set; }
        public string Buyer { get; private set; }
        public decimal Value { get; set; }
        public bool Suspended { get; set; }
        public DateTime Date { get; set; }

        public PurchaseOrder(string buyer, int count, decimal currentQuotation)
        {
            Id = _lastId++;
            Buyer = buyer;
            Count = count;
            Value = currentQuotation*count;
            Suspended = false;
            Date = DateTime.Now;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
