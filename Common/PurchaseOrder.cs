using System;
using OpenNETCF.ORM;

namespace Common
{
    [Entity(KeyScheme.Identity)]
    public class PurchaseOrder
    {
        [Field(IsPrimaryKey = true)]
        public int Id { get; private set; }

        [Field(FieldName = "Cnt")]
        public int Count { get; private set; }

        [Reference(typeof(User), "Id")]
        public User Buyer { get; private set; }
    
        public bool FulFilled { get; set; }

        public PurchaseOrder(User buyer, int count, bool fulfilled = false)
        {
            Id = -1;
            Buyer = buyer;
            Count = count;
            FulFilled = fulfilled;
        }

        public PurchaseOrder()
        {
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
