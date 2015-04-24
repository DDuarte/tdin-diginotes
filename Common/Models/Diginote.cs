using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class Diginote
    {
        private static int _lastId = 1;
        public int DiginoteId { get; private set; }
        public decimal Value { get; private set; }

        public User User { get; private set; }
        public SalesOrder SalesOrder { get; private set; }
        public Transaction Transaction { get; private set; }

        public Diginote()
        {
            DiginoteId = _lastId++;
            Value = 1;
        }

        public Diginote(decimal value)
        {
            DiginoteId = _lastId++;
            Value = value;
        }

        protected bool Equals(Diginote other)
        {
            return DiginoteId == other.DiginoteId;
        }

        public override int GetHashCode()
        {
            return DiginoteId;
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
