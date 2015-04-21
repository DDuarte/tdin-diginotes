﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Common;
using Remotes;

namespace Server
{
    // ReSharper disable once UnusedMember.Global
    public class DigiMarket : MarshalByRefObject, IDigiMarket
    {
        private readonly ActionLog _actionLog;

        public readonly decimal Quotation = 1;

        public readonly ConcurrentDictionary<String, User> Users = new ConcurrentDictionary<string, User>();

        public readonly List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
        public readonly List<SalesOrder> SalesOrders = new List<SalesOrder>();

        public DigiMarket()
        {
            _actionLog = new ActionLog(this);
        }

        private bool _applyingLogs = true;

        private Result<User> ValidateCredentials(string username, string password)
        {
            if (!Validators.ValidUsername(username))
                return new Result<User>(DigiMarketError.InvalidUsername);

            User user;
            if (!Users.TryGetValue(username, out user))
                return new Result<User>(DigiMarketError.UnexistingUser);

            if (!Validators.ValidPassword(password))
                return new Result<User>(DigiMarketError.InvalidPassword);

            if (user.PasswordHash != Utilities.ComputeHash(password))
                return new Result<User>(DigiMarketError.InvalidPassword);

            return new Result<User>(user);
        }

        public bool AddFunds(string username, string password, decimal euros)
        {
            Logger.Log("attempt: user={0} euros={1}", username, euros);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} euros={1} error={2}", username, euros, r.Error);
                return false;
            }

            r.Value.AddFunds(euros);
            PublishMessage(Update.Balance);

            Logger.Log("success: user={0} +balance={1}", username, euros);

            return true;
        }

        public Result<decimal> GetBalance(string username, string password)
        {
            Logger.Log("attempt: user={0}", username);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username,r.Error);
                return new Result<decimal>(r.Error);
            }

            var balance = r.Value.Balance;
            Logger.Log("success: user={0} balance={1}", username, balance);

            return new Result<decimal>(balance);
        }

        public void ApplyingLogs(bool active)
        {
            _applyingLogs = active;
        }

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }

        public event MessageArrivedEvent MessageArrived;

        public void PublishMessage(Update update)
        {
            SafeInvokeMessageArrived(update);
        }

        private void SafeInvokeMessageArrived(Update update)
        {
            if (MessageArrived == null)
                return; // no listeners

            MessageArrivedEvent listener = null;
            var dels = MessageArrived.GetInvocationList();

            foreach (var del in dels)
            {
                try
                {
                    listener = (MessageArrivedEvent)del;
                    listener.Invoke(update);
                }
                catch (Exception e)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    MessageArrived -= listener;
                }
            }
        }

        public Result<User> Register(string name, string username, string password)
        {
            Logger.Log("attempt: name={0} username={1} password={2}", name, username, password);

            var error = DigiMarketError.None;
            User user;

            if (!Validators.ValidUsername(username))
                error = DigiMarketError.InvalidUsername;
            else if (!Validators.ValidName(name))
                error = DigiMarketError.InvalidName;
            else if (!Validators.ValidPassword(password))
                error = DigiMarketError.InvalidPassword;
            else if (Users.TryGetValue(username, out user))
                error = DigiMarketError.ExistingUsername;

            if (error != DigiMarketError.None)
            {
                Logger.Log("fail: user={0} error={1}", username, error);
                return new Result<User>(error);
            }

            user = new User(name, username, password);
            Users.TryAdd(username, user);

            if (!_applyingLogs)
                _actionLog.LogAction(new NewUserAction { Name = username, User = username, Password = password });

            Logger.Log("success: user={0}", username);
            return new Result<User>(user);
        }

        public Result<User> Login(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);

            if (r && r.Value.LoggedIn)
                r = new Result<User>(DigiMarketError.AlreadyLoggedIn);

            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return r;
            }

            r.Value.LoggedIn = true;

            Logger.Log("success: user={0}", username);

            return r;
        }

        public Result<User> Logout(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);

            if (r && !r.Value.LoggedIn)
                r = new Result<User>(DigiMarketError.NotLoggedIn);

            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return r;
            }

            r.Value.LoggedIn = false;

            Logger.Log("success: user={0}", username);

            return r;
        }

        public Result<IEnumerable<Diginote>> GetDiginotes(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<IEnumerable<Diginote>>(r.Error);
            }

            var rr = new Result<IEnumerable<Diginote>>(Users[username].Diginotes);
            Logger.Log("success: user={0} diginotes={1}", username, rr.Value.Count());

            return rr;
        }

        public Result<List<PurchaseOrder>> GetPurchaseOrders(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<List<PurchaseOrder>>(r.Error);
            }

            var user = r.Value;

            var rr = new Result<List<PurchaseOrder>>(PurchaseOrders.Where(p => p.Buyer == user).ToList());
            Logger.Log("success: user={0} orders={1}", username, rr.Value.Count);

            return rr;
        }

        public Result<List<SalesOrder>> GetSalesOrders(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<List<SalesOrder>>(r.Error);
            }

            var user = r.Value;

            var rr = new Result<List<SalesOrder>>(SalesOrders.Where(s => s.Seller == user).ToList());
            Logger.Log("success: user={0} orders={1}", username, rr.Value.Count);

            return rr;
        }

        public PurchaseResult CreatePurchaseOrder(string username, string password, int quantity)
        {
            Logger.Log("attempt: username={0} password={1} quantity={2}", username, password, quantity);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return PurchaseResult.Error;
            }

            var requestingUser = r.Value;

            // get available offers
            var numOffers = SalesOrders.Where(order => !order.Fulfilled).Sum(order => order.Count);

            if (numOffers == 0)
            {
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, Quotation));
                PublishMessage(Update.General);
                return PurchaseResult.Unfulfilled;
            }

            var surplus = quantity - numOffers;
            var salesQuantity = 0;
            var availableDiginotes =
                SalesOrders
                    .Where((salesOrder) => !salesOrder.Fulfilled)
                    .TakeWhile((salesOrder) =>
                    {
                        bool exceeded = salesQuantity > quantity;
                        salesQuantity += salesOrder.Count;
                        return !exceeded;

                    });

            // purchase order is totally fulfilled
            if (surplus <= 0)
            {

                foreach (var salesOrder in SalesOrders)
                {
                    salesOrder.Fulfilled = true;
                    var selectedDiginotes = salesOrder.Diginotes.ToList();
                    selectedDiginotes.ForEach((selectedDiginote) => requestingUser.AddDiginote(selectedDiginote));
                    salesOrder.Diginotes.Clear();
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, Quotation, true));
                PublishMessage(Update.General);
                return PurchaseResult.Fulfilled;
            }
            else // the order is partially fulfilled
            {
                foreach (var salesOrder in SalesOrders)
                {
                    salesOrder.Fulfilled = true;
                    var selectedDiginotes = salesOrder.Diginotes.ToList();
                    selectedDiginotes.ForEach((selectedDiginote) => requestingUser.AddDiginote(selectedDiginote));
                    salesOrder.Diginotes.Clear();
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, numOffers, Quotation, true)); // fulfilled
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, surplus, Quotation)); // unfulfiled
                PublishMessage(Update.General);
                return PurchaseResult.PartiallyFullfilled;
            }
        }

        public bool UpdatePurchaseOrder(string username, string password, int id, decimal value)
        {
            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return false;
            }

            var user = r.Value;

            var purchaseOrder = PurchaseOrders.Where(order => order.Id == id && order.Buyer == user).ToList().FirstOrDefault();

            if (purchaseOrder == null)
                return false;

            purchaseOrder.Value = value;
            PublishMessage(Update.General);
            return true;
        }

        public void DeletePurchaseOrder(string username, string password, int id)
        {
            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return /* false */;
            }

            PurchaseOrder purchaseOrderToDelete = null;
            foreach (var order in PurchaseOrders)
            {
                if (order.Id == id)
                {
                    purchaseOrderToDelete = order;
                }
            }

            if (purchaseOrderToDelete == null) return;

            PurchaseOrders.Remove(purchaseOrderToDelete);
            PublishMessage(Update.General);
        }

        public void UpdateSaleOrder(string username, string password, int id, decimal value)
        {
            throw new NotImplementedException();
        }

        public void DeleteSaleOrder(string username, string password, int id)
        {
            throw new NotImplementedException();
        }

        public SalesResult CreateSalesOrder(string username, string password, int quantity)
        {
            Logger.Log("attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return SalesResult.Error;
            }

            var requestingUser = r.Value;
            if (requestingUser.Diginotes.Count < quantity)
                return SalesResult.InsufficientFunds;

            // get available offers
            var availablePurchaseOrders = PurchaseOrders.Where(order => !order.FulFilled && order.Buyer != requestingUser);
            var purchaseOrders = availablePurchaseOrders as IList<PurchaseOrder> ?? availablePurchaseOrders.ToList();
            var numOffers = purchaseOrders.Sum(  order => order.Count);

            if (numOffers == 0)
            {
                SalesOrders.Add(new SalesOrder(requestingUser, quantity, Quotation));
                return SalesResult.Unfulfilled;
            }

            var selectedPurchaseOrders = new List<PurchaseOrder>();
            for (int i = 0, purchaseQuantity = 0; i < purchaseOrders.Count() || purchaseQuantity < quantity; ++i)
            {
                var availablePurchaseOrder = purchaseOrders.ElementAt(i);
                var availableOrderCount = availablePurchaseOrder.Count;
                var excess = (purchaseQuantity + availableOrderCount) - quantity;
                if (excess <= 0)
                {
                    selectedPurchaseOrders.Add(availablePurchaseOrder);
                    purchaseQuantity += availableOrderCount;
                    continue;
                }

                var necessaryCount = quantity - purchaseQuantity;
                availablePurchaseOrder.Count = necessaryCount; 
                selectedPurchaseOrders.Add(availablePurchaseOrder);
                PurchaseOrders.Add(new PurchaseOrder(availablePurchaseOrder.Buyer, availableOrderCount - necessaryCount, Quotation));
            }

            // transfer diginotes
            foreach (var selectedPurchaseOrder in selectedPurchaseOrders)
            {
                selectedPurchaseOrder.FulFilled = true;
                var selectedDiginotes = requestingUser.Diginotes.Take(selectedPurchaseOrder.Count).ToList();
                selectedDiginotes.ForEach((selectedDiginote) => selectedPurchaseOrder.Buyer.AddDiginote(selectedDiginote));
                requestingUser.Diginotes.RemoveWhere((diginote) => selectedDiginotes.Contains(diginote));                 
            }

            PublishMessage(Update.General);
            if (selectedPurchaseOrders.Sum((order) => order.Count) < quantity)
                return SalesResult.PartiallyFullfilled;

            return SalesResult.Fulfilled;
        }
    }
}
