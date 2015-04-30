using System;
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

        private const int SuspendedTime = 15000;

        public decimal Quotation = 1;
        public readonly Dictionary<DateTime, decimal> QuotationHistory = new Dictionary<DateTime, decimal>
        {
            { DateTime.Now, 1}
        }; 

        public readonly ConcurrentDictionary<string, User> Users = new ConcurrentDictionary<string, User>();

        public readonly List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
        public readonly List<SalesOrder> SalesOrders = new List<SalesOrder>();
        public readonly List<Transaction> Transactions = new List<Transaction>();

        public DigiMarket()
        {
            _actionLog = new ActionLog(this);
        }

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }

        public event MessageArrivedEvent MessageArrived;

        public void PublishMessage(Update update)
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
                    // could not reach the destination, so remove it from the list
                    MessageArrived -= listener;
                }
            }
        }

        private bool _applyingLogs = true;

        public void ApplyingLogs(bool active)
        {
            _applyingLogs = active;
        }

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

            AddFundsDirect(r.Value.Username, euros, diginotes);

            PublishMessage(Update.Balance);
            PublishMessage(Update.Diginotes);

            Logger.Log("success: user={0} +balance={1} +diginotes={2}", username, euros, diginotes);

            return true;
        }

        public void AddFundsDirect(string user, decimal balance, int diginoteCount)
        {
            var u = Users[user];
            u.AddFunds(balance);
            for (var i = 0; i < diginoteCount; ++i)
                u.AddDiginote(new Diginote());

            if (!_applyingLogs)
                _actionLog.LogAction(new AddFundsAction { User = user, Balance = balance, DiginoteCount = diginoteCount });

            Logger.Log("success: user={0} balance={1} diginotes={2}", user, balance, diginoteCount);
        }

        private void OrdersSnapshot()
        {
            OrdersSnapshot(PurchaseOrders, SalesOrders);
        }

        public void OrdersSnapshot(List<PurchaseOrder> purchaseOrders, List<SalesOrder> salesOrders)
        {
            if (_applyingLogs)
            {
                PurchaseOrders.Clear();
                PurchaseOrders.AddRange(purchaseOrders);

                SalesOrders.Clear();
                SalesOrders.AddRange(salesOrders);

                PublishMessage(Update.General);
            }
            else
                _actionLog.LogAction(new OrdersSnapshotAction { PurchaseOrders = purchaseOrders, SalesOrders = salesOrders });

            Logger.Log("success: purchaseOrders#={0} salesOrders#={1}", purchaseOrders.Count, salesOrders.Count);
        }

        public void SetFundsDirect(string username, decimal balance)
        {
            User user;
            if (Users.TryGetValue(username, out user))
                user.SetBalance(balance);
        }

        public void UpdateDiginotesDirect(string username, int diginotes, int value)
        {
            User user;
            if (Users.TryGetValue(username, out user))
                user.UpdateDiginotes(diginotes, value);
        }

        public void ApplyTransaction(string buyer, string seller, int diginotes, decimal cost, DateTime date)
        {
            var buyerUser = Users[buyer];
            var sellerUser = Users[seller];

            var dns = sellerUser.Diginotes.Take(diginotes).ToList();
            foreach (var diginote in dns)
            {
                sellerUser.RemoveDiginote(diginote);
                buyerUser.AddDiginote(diginote);
            }

            sellerUser.AddFunds(cost);
            buyerUser.AddFunds(-cost);

            Transactions.Add(new Transaction(date, buyer, seller, dns, cost));
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

            ChangeQuotationDirect(quotation, DateTime.Now);

            var now = DateTime.Now;
            if (!QuotationHistory.ContainsKey(now))
                QuotationHistory.Add(now, Quotation);

            PublishMessage(Update.Quotation);

            foreach (var order in PurchaseOrders)
                order.Suspended = true;

            PublishMessage(Update.General);

            await Task.Delay(SuspendedTime);

            foreach (var order in PurchaseOrders) // TODO: need update (match orders)
                order.Suspended = false;

            PublishMessage(Update.General);
        }

        private async Task ChangeQuotationSales(decimal quotation, SalesOrder selectedOrder)
        {
            selectedOrder.Value = quotation * selectedOrder.Count;

            ChangeQuotationDirect(quotation, DateTime.Now);

            PublishMessage(Update.Quotation);

            foreach (var order in SalesOrders)
                order.Suspended = true;

            PublishMessage(Update.General);

            await Task.Delay(SuspendedTime);

            foreach (var order in SalesOrders) // TODO: need update (match orders)
                order.Suspended = false;

            PublishMessage(Update.General);
        }

        public void ChangeQuotationDirect(decimal quotation, DateTime time)
        {
            Quotation = quotation;
            QuotationHistory[time] = quotation;

            if (!_applyingLogs)
                _actionLog.LogAction(new QuotationChangeAction { Quotation = quotation, Time = time });

            Logger.Log("success: quotation={0} time={1}", quotation, time);
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

            var rr = new Result<List<PurchaseOrder>>(PurchaseOrders.Where(p => p.Buyer == user.Username).ToList());
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

            var rr = new Result<List<SalesOrder>>(SalesOrders.Where(s => s.Seller == user.Username).ToList());
            Logger.Log("success: user={0} orders={1}", username, rr.Value.Count);

            return rr;
        }

        public Result<List<Transaction>> GetTransactions(string username, string password)
        {
            Logger.Log("attempt: username={0} password={1}", username, password);

            var r = ValidateCredentials(username, password);
            if (!r)
            {
                Logger.Log("fail: user={0} error={1}", username, r.Error);
                return new Result<List<Transaction>>(r.Error);
            }

            var user = r.Value;

            var rr = new Result<List<Transaction>>(Transactions.Where(t => t.Seller == user.Username || t.Buyer == user.Username).ToList());
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

            requestingUser.AddFunds(-price);
            var newPurchaseOrder = new PurchaseOrder(requestingUser.Name, quantity, Quotation);
            PurchaseOrders.Add(newPurchaseOrder);

            var updated = UpdateOrders();

            PublishMessage(Update.Balance);

            if (updated)
            {
                PublishMessage(Update.General);
                PublishMessage(Update.Diginotes);

                if (PurchaseOrders.Contains(newPurchaseOrder)) // check if purchase order has been partially fulfilled
                {
                    return new Result<PurchaseOrder>(newPurchaseOrder, DigiMarketError.NotFullfilled);
                }
                else
                {
                    return new Result<PurchaseOrder>(newPurchaseOrder);
                }
            }
            else // no updates in the system orders, this one is surely not fulfilled (unless some voodoo mumbo jumbo happened)
            {
                return new Result<PurchaseOrder>(newPurchaseOrder, DigiMarketError.NotFullfilled);
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

            var purchaseOrder = PurchaseOrders.Where(order => order.Id == id && order.Buyer == user.Username).ToList().FirstOrDefault();

            if (purchaseOrder == null)
                return false;

            purchaseOrder.Value = value; OrdersSnapshot();
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

            Users[purchaseOrderToDelete.Buyer].AddFunds(purchaseOrderToDelete.Value);
            if (!_applyingLogs)
                _actionLog.LogAction(new UpdateBalanceAction { User = purchaseOrderToDelete.Buyer, Balance = Users[purchaseOrderToDelete.Buyer].Balance });

            PurchaseOrders.Remove(purchaseOrderToDelete); OrdersSnapshot();
            PublishMessage(Update.Balance);
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

            var saleOrder = SalesOrders.Where(order => order.Id == id && order.Seller == user.Username).ToList().FirstOrDefault();

            if (saleOrder == null)
                return false;

            saleOrder.Value = value; OrdersSnapshot();
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

            if (saleOrderToDelete == null) return;

            saleOrderToDelete.Diginotes.ToList().ForEach(diginote => Users[saleOrderToDelete.Seller].AddDiginote(diginote));
            if (!_applyingLogs)
                _actionLog.LogAction(new UpdateDiginotesAction() { Diginotes = saleOrderToDelete.Diginotes.Count, User = saleOrderToDelete.Seller, Value = saleOrderToDelete.Diginotes.Count > 0 ? saleOrderToDelete.Diginotes.First().Value : 1 });

            SalesOrders.Remove(saleOrderToDelete); OrdersSnapshot();
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

            var selectedDiginotes = requestingUser.Diginotes.Take(quantity).ToList();
            var newSalesOrder = new SalesOrder(requestingUser, Quotation, selectedDiginotes);
            requestingUser.Diginotes.RemoveWhere(diginote => selectedDiginotes.Contains(diginote));
            SalesOrders.Add(newSalesOrder);
            PublishMessage(Update.Diginotes);
            
            var updated = UpdateOrders();

            PublishMessage(Update.Diginotes);

            if (updated)
            {
                PublishMessage(Update.Balance);
                PublishMessage(Update.General);

                if (SalesOrders.Contains(newSalesOrder)) // check if purchase order has been partially fulfilled
                {
                    return new Result<SalesOrder>(newSalesOrder, DigiMarketError.NotFullfilled);
                }
                else
                {
                    return new Result<SalesOrder>(newSalesOrder);
                }
            }
            else // no updates in the system orders, this one is surely not fulfilled (unless some voodoo mumbo jumbo happened)
            {
                return new Result<SalesOrder>(newSalesOrder, DigiMarketError.NotFullfilled);
            }
        }

        private bool UpdateOrders()
        {
            var updateOcurred = false;

            // match orders
            var openPurchaseOrders = PurchaseOrders
                .Where(order => !order.Suspended)
                .OrderBy(order => order.Date).ToList();

            var openSalesOrders = SalesOrders
                .Where(order => !order.Suspended)
                .OrderBy(order => order.Date).ToList();

            if (!openPurchaseOrders.Any() || !openSalesOrders.Any()) // no orders to match
            {
                return false;
            }

            foreach (var openPurchaseOrder in openPurchaseOrders)
            {
                openSalesOrders = SalesOrders
                    .Where(order => !order.Suspended && order.Seller != openPurchaseOrder.Buyer)
                    .OrderBy(order => order.Date).ToList();

                var currentQuantity = 0;
                SalesOrder splitSalesOrder;
                var selectedSalesOrders = openSalesOrders
                    .TakeWhile(salesOrder =>
                    {
                        bool exceeded = currentQuantity > openPurchaseOrder.Count;
                        currentQuantity += salesOrder.Count;

                        var excess = currentQuantity - openPurchaseOrder.Count;

                        if (excess > 0) // there's an excess, we have to split a sales order
                        {
                            var necessaryCount = openPurchaseOrder.Count - (currentQuantity - salesOrder.Count);
                            var transientDiginotes = salesOrder.Diginotes.Take(salesOrder.Count - necessaryCount).ToList();
                            salesOrder.Diginotes.RemoveWhere(diginote => transientDiginotes.Contains(diginote));

                            splitSalesOrder = new SalesOrder(Users[salesOrder.Seller], Quotation, transientDiginotes);
                            SalesOrders.Add(splitSalesOrder);
                        }

                        return !exceeded;
                    }).ToList();

                // partially fulfilled
                if (selectedSalesOrders.Sum(order => order.Count) < openPurchaseOrder.Count)
                {
                    var unfulfilledQuantity = openPurchaseOrder.Count - selectedSalesOrders.Sum(order => order.Count);
                    PurchaseOrders.Add(new PurchaseOrder(openPurchaseOrder.Buyer, unfulfilledQuantity, Quotation));
                }
                
                PurchaseOrders.Remove(openPurchaseOrder);

                foreach (var selectedSalesOrder in selectedSalesOrders)
                {
                    updateOcurred = true;
                    Transactions.Add(new Transaction(DateTime.Now, openPurchaseOrder.Buyer,
                        selectedSalesOrder.Seller, selectedSalesOrder.Diginotes.ToList(), selectedSalesOrder.Value));


                    if (!_applyingLogs)
                        _actionLog.LogAction(new TransactionAction
                        {
                            Buyer = openPurchaseOrder.Buyer,
                            Seller = selectedSalesOrder.Seller,
                            Cost = selectedSalesOrder.Value,
                            Date = DateTime.Now,
                            Diginotes = selectedSalesOrder.Diginotes.Count
                        });

                    foreach (var diginote in selectedSalesOrder.Diginotes) // transfer diginotes to buyer
                    {
                        Users[openPurchaseOrder.Buyer].Diginotes.Add(diginote);
                    }
                    Users[selectedSalesOrder.Seller].AddFunds(selectedSalesOrder.Value); // transfer funds to seller
                    SalesOrders.Remove(selectedSalesOrder);
                }
            }

            OrdersSnapshot();

            return updateOcurred;
        }
    }
}
