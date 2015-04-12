using System;
using OpenNETCF.ORM;

namespace Common
{
    [Entity(KeyScheme.Identity)]
    public class Diginote : MarshalByRefObject
    {
        [Field(IsPrimaryKey = true)]
        public Guid Id { get; private set; }

        [Field]
        public int Value { get; set; }

        public Diginote()
        {
            Id = Guid.NewGuid();
            Value = 1;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
