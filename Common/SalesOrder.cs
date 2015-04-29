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

        public int Count
        {
            get
            {
                return Diginotes.Count;
            }
        }

        public string Seller { get; set; }
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

        public SalesOrder(User seller, decimal currentQuotation, List<Diginote> diginotes)
        {
            Id = _lastId++;
            Seller = seller.Username;
            Suspended = false;
            Diginotes = new HashSet<Diginote>();
            Date = DateTime.Now;
            Diginotes = new HashSet<Diginote>(diginotes);

            _quotation = currentQuotation;
            _valueOverriden = false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
