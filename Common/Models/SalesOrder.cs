using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Common.Models
{
    public class SalesOrder
    {
        private static int _lastId = 1;
        public int SalesOrderId { get; private set; }
        public int Count { get; private set; }
        public User Seller { get; private set; }
        public DateTime Date { get; private set; }
        public decimal Value { get; set; }
        public IList<Diginote> Diginotes { get; set; }
        public bool Suspended { get; set; }

        public SalesOrder(User seller, int count, decimal currentQuotation)
        {
            SalesOrderId = _lastId++;
            Count = count;
            Seller = seller;
            Suspended = false;
            Diginotes = new List<Diginote>();
            Date = DateTime.Now;

            Diginotes = new List<Diginote>(seller.Diginotes.Take(count));
        }
    }
}
