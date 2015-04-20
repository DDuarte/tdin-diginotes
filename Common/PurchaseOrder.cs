﻿using System;

namespace Common
{
    [Serializable]
    public class PurchaseOrder
    {
        private static int _lastId;
        public int Id { get; private set; }
        public int Count { get; private set; }
        public User Buyer { get; private set; }
        public bool FulFilled { get; set; }
        public decimal Value { get; set; }

        public PurchaseOrder(User buyer, int count, decimal currentQuotation, bool fulfilled = false)
        {
            Id = _lastId++;
            Buyer = buyer;
            Count = count;
            FulFilled = fulfilled;
            Value = currentQuotation*count;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
