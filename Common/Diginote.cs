using System;
using OpenNETCF.ORM;

namespace Common
{
    [Entity(KeyScheme.Identity)]
    public class Diginote : MarshalByRefObject
    {
        [Field(IsPrimaryKey = true)]
        public int Id { get; private set; }

        [Field]
        public int Value { get; set; }

        [Field]
        public int UserGuid { get; set; }

        public Diginote()
        {
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
