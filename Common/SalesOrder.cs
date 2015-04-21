﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    [Serializable]
    public class SalesOrder : MarshalByRefObject
    {
        private static int _lastId;
        public int Id { get; private set; }
        public int Count { get; private set; }
        public User Seller { get; private set; }
        public bool Fulfilled { get; set; }
        public decimal Value {get; set;}
        public HashSet<Diginote> Diginotes { get; set; }

        public SalesOrder(User seller, int count, decimal currentQuotation, bool fulfilled = false)
        {
            Id = _lastId++;
            Count = count;
            Seller = seller;
            Value = currentQuotation*count;
            Fulfilled = fulfilled;
            Diginotes = new HashSet<Diginote>(Seller.Diginotes.Take(count));

            if (Diginotes.Count < count)
                throw new Exception("User has insufficient diginotes");

            seller.Diginotes.RemoveWhere((diginote) => Diginotes.Contains(diginote));
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
