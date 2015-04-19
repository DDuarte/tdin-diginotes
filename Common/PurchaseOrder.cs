using System;

namespace Common
{
    [Serializable]
    public class PurchaseOrder
    {
        public Guid Id { get; private set; }
        public int Count { get; private set; }
        public User Buyer { get; private set; }
        public bool FulFilled { get; set; }

        public PurchaseOrder(User buyer, int count, bool fulfilled = false)
        {
            Id = Guid.NewGuid();
            Buyer = buyer;
            Count = count;
            FulFilled = fulfilled;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
