using System;
using OpenNETCF.ORM;

namespace Common
{
    [Entity(KeyScheme.Identity)]
    public class SalesOrder : MarshalByRefObject
    {
        [Field(IsPrimaryKey = true)]
        public Guid Id { get; private set; }

        [Field(FieldName = "Cnt")]
        public int Count { get; private set; }

        [Reference(typeof(User), "Id")]
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
