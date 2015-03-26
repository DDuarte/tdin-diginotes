using System;

namespace Common
{
    public class SalesOrder : MarshalByRefObject
    {
        public int Count { get; private set; }
        public User Seller { get; private set; }
        public bool Fulfilled { get; set; }

        public SalesOrder(User seller, int count)
        {
            Count = count;
            Seller = seller;
            Fulfilled = false;
        }
    }
}
