using System;

namespace Common
{
    [Serializable]
    public class SalesOrder : MarshalByRefObject
    {
        public Guid Id { get; private set; }
        public int Count { get; private set; }
        public User Seller { get; private set; }
        public bool Fulfilled { get; set; }

        public SalesOrder(User seller, int count, bool fulfilled = false)
        {
            Id = Guid.NewGuid();
            Count = count;
            Seller = seller;
            Fulfilled = fulfilled;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
