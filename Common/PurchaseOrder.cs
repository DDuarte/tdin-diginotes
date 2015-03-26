namespace Common
{
    public class PurchaseOrder
    {
        public int Count { get; private set; }
        public User Buyer { get; private set; }
        public bool FulFilled { get; set; }

        public PurchaseOrder(User buyer, int count)
        {
            Buyer = buyer;
            Count = count;
            FulFilled = false;
        }
    }
}
