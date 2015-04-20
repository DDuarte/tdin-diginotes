using System;

namespace Common
{
    [Serializable]
    public class SalesOrder : MarshalByRefObject
    {
        private static int _lastId;
        public int Id { get; private set; }
        public int Count { get; private set; }
        public User Seller { get; private set; }
        public bool Fulfilled { get; private set; }
        public decimal Value {get; set;}

        public SalesOrder(User seller, int count, decimal currentQuotation, bool fulfilled = false)
        {
            Id = _lastId++;
            Count = count;
            Seller = seller;
            Value = currentQuotation*count;
            Fulfilled = fulfilled;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
