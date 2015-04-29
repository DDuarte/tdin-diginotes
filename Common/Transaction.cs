using System;
using System.Collections.Generic;

namespace Common
{
    [Serializable]
    public class Transaction
    {
        private static int _lastId = 1;

        public int TransactionId { get; private set; }
        public DateTime Date { get; private set; }
        public string Buyer { get; private set; }
        public string Seller { get; private set; }
        public IList<Diginote> Diginotes { get; private set; }
        public decimal Cost { get; private set; }

        public int DiginotesCount
        {
            get { return Diginotes.Count; }
        }

        public Transaction(DateTime date, string buyer, string seller, IList<Diginote> diginotes, decimal cost)
        {
            TransactionId = _lastId++;
            Date = date;
            Buyer = buyer;
            Seller = seller;
            Diginotes = diginotes;
            Cost = cost;
        }
    }
}
