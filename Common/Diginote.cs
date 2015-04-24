using System;
using System.Collections.Generic;

namespace Common
{
    public class Diginote
    {
        private static int _lastId = 1;
        public int Id { get; private set; }
        public int Value { get; private set; }

        public Diginote()
        {
            Id = _lastId++;
            Value = 1;
        }

        protected bool Equals(Diginote other)
        {
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Diginote;
            return other != null && Equals(other);
        }
    }
}
