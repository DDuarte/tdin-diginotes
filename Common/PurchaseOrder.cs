using System;

namespace Common
{
    [Serializable]
    public class PurchaseOrder
    {
        private static int _lastId = 1;
        public int Id { get; private set; }
        public int Count { get; set; }
        public User Buyer { get; private set; }
        public bool FulFilled { get; set; }
        public decimal Value { get; set; }
        public bool Suspended { get; set; }

        public PurchaseOrder(User buyer, int count, decimal currentQuotation, bool fulfilled = false)
        {
            Id = _lastId++;
            Buyer = buyer;
            Count = count;
            FulFilled = fulfilled;
            Value = currentQuotation*count;
            Suspended = false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
