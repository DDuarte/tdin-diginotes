﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Remotes;

namespace Server
{
    // ReSharper disable once UnusedMember.Global
    public class DigiMarket : MarshalByRefObject, IDigiMarket
    {
        private readonly ActionLog _actionLog;

        private const int SuspendedTime = 60000;

        public decimal Quotation = 1;
        public readonly Dictionary<DateTime, decimal> QuotationHistory = new Dictionary<DateTime, decimal>
        {
            { DateTime.Now, 1}
        }; 

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

        public bool AddFunds(string username, string password, decimal euros, int diginotes)
        {
            Logger.Log("attempt: user={0} euros={1} diginotes={2}", username, euros, diginotes);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} euros={1} diginotes={2} error={3}", username, euros, diginotes, r.Error);
                return false;
            }

            r.Value.AddFunds(euros);
            for (var i = 0; i < diginotes; ++i)
                r.Value.AddDiginote(new Diginote());

            PublishMessage(Update.Balance);
            PublishMessage(Update.Diginotes);

            Logger.Log("success: user={0} +balance={1} +diginotes={2}", username, euros, diginotes);

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

        public Result<decimal> GetQuotation(string username, string password)
        {
            Logger.Log("attempt: user={0}", username);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<decimal>(r.Error);
            }

            return new Result<decimal>(Quotation);
        }

        public bool ChangeQuotation(string username, string password, decimal quotation, int id, bool isPurchase)
        {
            Logger.Log("attempt: user={0}", username);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return false;
            }

            if (isPurchase)
            {
                if (quotation < Quotation)
                    return false;

                PurchaseOrder selectedOrder = PurchaseOrders.FirstOrDefault(order => order.Id == id);
                if (selectedOrder == null)
                    return false;

                Task.Run(() => ChangeQuotationPurchase(quotation, selectedOrder));
                
                return true;
            }
            else
            {
                if (quotation > Quotation)
                    return false;

                SalesOrder selectedOrder = SalesOrders.FirstOrDefault(order => order.Id == id);
                if (selectedOrder == null)
                    return false;

                Task.Run(() => ChangeQuotationSales(quotation, selectedOrder));

                return true;
            }
        }

        private async Task ChangeQuotationPurchase(decimal quotation, PurchaseOrder selectedOrder)
        {
            selectedOrder.Value = quotation*selectedOrder.Count;

            Quotation = quotation;

            var now = DateTime.Now;
            if (!QuotationHistory.ContainsKey(now))
                QuotationHistory.Add(now, Quotation);

            PublishMessage(Update.Quotation);

            foreach (var purchaseOrder in PurchaseOrders)
            {
                purchaseOrder.Suspended = true;
            }

            PublishMessage(Update.General);

            await Task.Delay(SuspendedTime);

            foreach (var purchaseOrder in PurchaseOrders)
            {
                purchaseOrder.Suspended = false;
                // TODO: need update (match orders)
            }

            PublishMessage(Update.General);
        }

        private async Task ChangeQuotationSales(decimal quotation, SalesOrder selectedOrder)
        {
            selectedOrder.Value = quotation * selectedOrder.Count;

            var now = DateTime.Now;
            if (!QuotationHistory.ContainsKey(now))
                QuotationHistory.Add(now, Quotation);

            PublishMessage(Update.Quotation);

            foreach (var order in SalesOrders)
                order.Suspended = true;

            PublishMessage(Update.General);

            await Task.Delay(SuspendedTime);

            foreach (var order in SalesOrders) // TODO: need update (match orders)
                order.Suspended = false;

            PublishMessage(Update.General);
        }

        public Dictionary<DateTime, decimal> GetQuotationHistory(string username, string password)
        {
            Logger.Log("attempt: user={0}", username);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return null;
            }

            return QuotationHistory;
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
                    Logger.Log("exception: {0}: {1}", update, e.ToString());
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

        public Result<List<Diginote>> GetDiginotes(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<List<Diginote>>(r.Error);
            }

            var rr = new Result<List<Diginote>>(Users[username].Diginotes.ToList());
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

        public Result<PurchaseOrder> CreatePurchaseOrder(string username, string password, int quantity)
        {
            Logger.Log("attempt: username={0} password={1} quantity={2}", username, password, quantity);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<PurchaseOrder>(r.Error);
            }

            var requestingUser = r.Value;

            var price = quantity*Quotation;
            if (requestingUser.Balance < price)
                return new Result<PurchaseOrder>(DigiMarketError.InsuficientFunds);

            // get available offers
            var numOffers = SalesOrders.Where(order => !order.Fulfilled && !order.Suspended).Sum(order => order.Count);

            if (numOffers == 0)
            {
                var po = new PurchaseOrder(requestingUser, quantity, Quotation);
                PurchaseOrders.Add(po);
                requestingUser.AddFunds(-po.Value);
                PublishMessage(Update.General);
                PublishMessage(Update.Balance);
                return new Result<PurchaseOrder>(po, DigiMarketError.NotFullfilled);
            }

            var surplus = quantity - numOffers;
            var salesQuantity = 0;
            var salesOrdersToAdd = new List<SalesOrder>();
            var selectedSalesOrders =
                SalesOrders
                    .Where(salesOrder => !salesOrder.Fulfilled && !salesOrder.Suspended /* && salesOrder.Seller != requestingUser */)
                    .TakeWhile(salesOrder =>
                    {
                        bool exceeded = salesQuantity > quantity;
                        salesQuantity += salesOrder.Count;

                        var excess = salesQuantity - quantity;
                        if (excess > 0)
                        {
                            var necessaryCount = quantity - (salesQuantity - salesOrder.Count);
                            var transientDiginotes = salesOrder.Diginotes.Take(salesOrder.Count - necessaryCount).ToList();
                            transientDiginotes.ForEach(transientDiginote => salesOrder.Seller.AddDiginote(transientDiginote));
                            salesOrder.Diginotes.RemoveWhere((diginote) => transientDiginotes.Contains(diginote));
                            salesOrdersToAdd.Add(new SalesOrder(salesOrder.Seller, salesOrder.Count - necessaryCount, Quotation));
                            salesOrder.Count = necessaryCount;
                        }

                        return !exceeded;
                    }).ToList();

            SalesOrders.AddRange(salesOrdersToAdd);
            // purchase order is totally fulfilled
            if (surplus <= 0)
            {

                foreach (var salesOrder in selectedSalesOrders)
                {
                    salesOrder.Fulfilled = true;
                    var selectedDiginotes = salesOrder.Diginotes.ToList();
                    selectedDiginotes.ForEach(selectedDiginote => requestingUser.AddDiginote(selectedDiginote));
                    salesOrder.Diginotes.Clear();
                }

                var fulfilledPurchaseOrder = new PurchaseOrder(requestingUser, quantity, Quotation, true);
                PurchaseOrders.Add(fulfilledPurchaseOrder);
                requestingUser.AddFunds(-fulfilledPurchaseOrder.Value);
                fulfilledPurchaseOrder.Buyer.AddFunds(fulfilledPurchaseOrder.Value);
                PublishMessage(Update.Balance);
                PublishMessage(Update.General);
                PublishMessage(Update.Diginotes);
                return new Result<PurchaseOrder>(fulfilledPurchaseOrder);
            }
            else // the order is partially fulfilled
            {
                foreach (var salesOrder in selectedSalesOrders)
                {
                    salesOrder.Fulfilled = true;
                    var selectedDiginotes = salesOrder.Diginotes.ToList();
                    selectedDiginotes.ForEach(selectedDiginote => requestingUser.AddDiginote(selectedDiginote));
                    salesOrder.Diginotes.Clear();
                }

                var fulfilledPurchaseOrder = new PurchaseOrder(requestingUser, numOffers, Quotation, true);
                var unfulfilledPurchaseOrder = new PurchaseOrder(requestingUser, surplus, Quotation);
                PurchaseOrders.Add(fulfilledPurchaseOrder); // fulfilled
                PurchaseOrders.Add(unfulfilledPurchaseOrder); // unfulfiled
                requestingUser.AddFunds(-fulfilledPurchaseOrder.Value);
                fulfilledPurchaseOrder.Buyer.AddFunds(fulfilledPurchaseOrder.Value);
                PublishMessage(Update.Balance);
                PublishMessage(Update.General);
                PublishMessage(Update.Diginotes);
                return new Result<PurchaseOrder>(unfulfilledPurchaseOrder, DigiMarketError.NotFullfilled);
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

            if (purchaseOrderToDelete == null || purchaseOrderToDelete.FulFilled) return;

            PurchaseOrders.Remove(purchaseOrderToDelete);
            PublishMessage(Update.General);
        }

        public bool UpdateSaleOrder(string username, string password, int id, decimal value)
        {
            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return false;
            }

            var user = r.Value;

            var saleOrder = SalesOrders.Where(order => order.Id == id && order.Seller == user).ToList().FirstOrDefault();

            if (saleOrder == null)
                return false;

            saleOrder.Value = value;
            PublishMessage(Update.General);
            return true;
        }

        public void DeleteSaleOrder(string username, string password, int id)
        {
            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return /* false */;
            }

            SalesOrder saleOrderToDelete = null;
            foreach (var order in SalesOrders)
            {
                if (order.Id == id)
                {
                    saleOrderToDelete = order;
                }
            }

            if (saleOrderToDelete == null || saleOrderToDelete.Fulfilled) return;

            saleOrderToDelete.Diginotes.ToList().ForEach(diginote => saleOrderToDelete.Seller.AddDiginote(diginote));
            SalesOrders.Remove(saleOrderToDelete);
            PublishMessage(Update.General);
            PublishMessage(Update.Diginotes);
        }

        public Result<SalesOrder> CreateSalesOrder(string username, string password, int quantity)
        {
            Logger.Log("attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<SalesOrder>(r.Error);
            }

            var requestingUser = r.Value;
            if (requestingUser.Diginotes.Count < quantity)
                return new Result<SalesOrder>(DigiMarketError.InsuficientFunds);

            // get available offers
            var availablePurchaseOrders = PurchaseOrders.Where(order => !order.FulFilled && !order.Suspended /* && order.Buyer != requestingUser */);
            var purchaseOrders = availablePurchaseOrders as IList<PurchaseOrder> ?? availablePurchaseOrders.ToList();

            var numOffers = purchaseOrders.Sum(order => order.Count);
            var surplus = quantity - numOffers;

            if (numOffers == 0)
            {
                var order = new SalesOrder(requestingUser, quantity, Quotation);
                SalesOrders.Add(order);
                PublishMessage(Update.Diginotes);
                return new Result<SalesOrder>(order, DigiMarketError.NotFullfilled);
            }

            // select orders
            var selectedPurchaseOrders = new List<PurchaseOrder>();
            for (int i = 0, purchaseQuantity = 0; i < purchaseOrders.Count() && purchaseQuantity < quantity; ++i)
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

                // split purchase order
                var necessaryCount = quantity - purchaseQuantity;
                availablePurchaseOrder.Value = necessaryCount * availablePurchaseOrder.Value / availablePurchaseOrder.Count;
                availablePurchaseOrder.Count = necessaryCount;
                selectedPurchaseOrders.Add(availablePurchaseOrder);
                PurchaseOrders.Add(new PurchaseOrder(availablePurchaseOrder.Buyer, availableOrderCount - necessaryCount, Quotation));
            }

            // transfer diginotes
            foreach (var selectedPurchaseOrder in selectedPurchaseOrders)
            {
                selectedPurchaseOrder.FulFilled = true;
                requestingUser.AddFunds(selectedPurchaseOrder.Value);
                var selectedDiginotes = requestingUser.Diginotes.Take(selectedPurchaseOrder.Count).ToList();
                requestingUser.Diginotes.RemoveWhere(diginote => selectedDiginotes.Contains(diginote));
                selectedDiginotes.ForEach(selectedDiginote => selectedPurchaseOrder.Buyer.AddDiginote(selectedDiginote));                 
            }

            // SalesOrder is totally fulfilled
            if (surplus <= 0)
            {
                var fulfilledSalesOrder = new SalesOrder(requestingUser, quantity, Quotation, true);
                SalesOrders.Add(fulfilledSalesOrder);
                PublishMessage(Update.Balance);
                PublishMessage(Update.General);
                PublishMessage(Update.Diginotes);
                return new Result<SalesOrder>(fulfilledSalesOrder);
            }
            else
            {
                var unfulfilledSalesOrder = new SalesOrder(requestingUser, surplus, Quotation);
                SalesOrders.Add(new SalesOrder(requestingUser, numOffers, Quotation, true));
                SalesOrders.Add(unfulfilledSalesOrder);
                PublishMessage(Update.Balance);
                PublishMessage(Update.General);
                PublishMessage(Update.Diginotes);
                return new Result<SalesOrder>(unfulfilledSalesOrder, DigiMarketError.NotFullfilled);
            }
        }
    }
}
