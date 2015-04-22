using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    [Serializable]
    public class SalesOrder
    {
        private static int _lastId = 1;
        public int Id { get; set; }
        public int Count { get; set; }
        public string Seller { get; set; }
        public bool Fulfilled { get; set; }
        public DateTime Date { get; set; }

        private bool _valueOverriden;
        private decimal _newValue;

        public decimal Value
        {
            get
            {
                if (!_valueOverriden)
                    return Count*_quotation;
                return _newValue;
            }
            set 
            { 
                _valueOverriden = true;
                _newValue = value;
            }
        }

        public HashSet<Diginote> Diginotes { get; set; }
        public bool Suspended { get; set; }

        private readonly decimal _quotation;

        public SalesOrder()
        {
            Diginotes = new HashSet<Diginote>();
            Date = DateTime.Now;
        }

        public SalesOrder(User seller, int count, decimal currentQuotation, bool fulfilled = false)
        {
            Id = _lastId++;
            Count = count;
            Seller = seller.Username;
            Fulfilled = fulfilled;
            Suspended = false;
            Diginotes = new HashSet<Diginote>();
            Date = DateTime.Now;

            _quotation = currentQuotation;
            _valueOverriden = false;

            if (fulfilled) return;

            Diginotes = new HashSet<Diginote>(seller.Diginotes.Take(count));
            if (Diginotes.Count < count)
                throw new Exception("User has insufficient diginotes");

            seller.Diginotes.RemoveWhere(diginote => Diginotes.Contains(diginote));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
