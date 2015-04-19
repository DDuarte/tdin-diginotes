using System;

namespace Common
{
    public class Diginote : MarshalByRefObject
    {
        public Guid Id { get; private set; }
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
