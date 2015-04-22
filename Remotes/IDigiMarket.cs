﻿using System;
using System.Collections.Generic;
using Common;

namespace Remotes
{
    public interface IDigiMarket
    {
        #region Events

        event MessageArrivedEvent MessageArrived;

        #endregion

        void PublishMessage(Update update);
        Result<User> Register(string name, string username, string password);
        Result<User> Login(string username, string password);
        Result<User> Logout(string username, string password);
        Result<PurchaseOrder> CreatePurchaseOrder(string username, string password, int quantity);
        bool UpdatePurchaseOrder(string username, string password, int id, decimal value);
        void DeletePurchaseOrder(string username, string password, int id);
        bool UpdateSaleOrder(string username, string password, int id, decimal value);
        void DeleteSaleOrder(string username, string password, int id);
        Result<SalesOrder> CreateSalesOrder(string username, string password, int quantity);
        Result<List<PurchaseOrder>> GetPurchaseOrders(string username, string password);
        Result<List<SalesOrder>> GetSalesOrders(string username, string password);
        bool AddFunds(string username, string password, decimal euros, int diginotes);
        Result<decimal> GetBalance(string username, string password);
        Result<List<Diginote>> GetDiginotes(string username, string password);
        Result<decimal> GetQuotation(string username, string password);
        bool ChangeQuotation(string username, string password, decimal quotation, bool isPurchase);  

        void ApplyingLogs(bool active);
    }

    public enum DigiMarketError
    {
        None,
        AlreadyLoggedIn,
        ExistingUsername,
        InvalidName,
        InvalidPassword,
        InvalidUsername,
        NotLoggedIn,
        UnexistingUser,
        NotFullfilled,
        InsuficientFunds
    }

    [Serializable]
    public class Result<T>
    {
        public T Value { get; private set; }
        public DigiMarketError Error { get; private set; }

        public Result(DigiMarketError error)
        {
            Value = default(T);
            Error = error;
        }

        public Result(T value, DigiMarketError error)
        {
            Value = value;
            Error = error;
        } 

        public Result(T value)
        {
            Value = value;
            Error = DigiMarketError.None;
        }

        public static implicit operator bool(Result<T> r)
        {
            return r.Error == DigiMarketError.None;
        }
    }
}
