using System;

namespace Common
{
    [Serializable]
    public class Diginote : MarshalByRefObject
    {
        private static int _lastId = 1;
        public int Id { get; private set; }
        public int Value { get; set; }

        public Diginote()
        {
            Id = _lastId++;
            Value = 1;
        }

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
