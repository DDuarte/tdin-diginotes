using System;

namespace Common
{
    public class Diginote : MarshalByRefObject
    {
        private static int _lastId;
        public int Id { get; private set; }
        public int Value { get; set; }

        public Diginote()
        {
            Id = _lastId++;
            Value = 1;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
