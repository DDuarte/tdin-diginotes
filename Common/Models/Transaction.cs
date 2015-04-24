using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class Transaction
    {
        public int TransactionId { get; private set; }
        public DateTime Date { get; private set; }
        public User Buyer { get; private set; }
        public User Seller { get; private set; }
        public IList<Diginote> Diginotes { get; private set; }
        public decimal Cost { get; private set; }
    }
}
