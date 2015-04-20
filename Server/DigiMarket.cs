using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public bool AddFunds(string username, string password, decimal euros)
        {
            Logger.Log("AddFunds", "attempt: user={0} euros={1}", username, euros);

            // insert login logic here

            Users[username].AddFunds(euros);
            PublishMessage(Update.Balance);

            Logger.Log("AddFunds", "user={0} euros={1}", username, euros);

            return true;
        }

        public decimal GetBalance(string username, string password)
        {
            Logger.Log("GetBalance", "attempt: user={0}", username);

            // insert login logic here

            var balance = Users[username].Balance;
            Logger.Log("GetBalance", "user={0}", username);

            return balance;
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
                catch (Exception)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    MessageArrived -= listener;
                }
            }
        }

        public RegisterError Register(string name, string username, string password)
        {
            Logger.Log("Register", "attempt: name={0} username={1} password={2}", name, username, password);

            if (String.IsNullOrWhiteSpace(name))
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - null or whitespace username", name, username, password);
                return RegisterError.InvalidName;
            }

            if (String.IsNullOrWhiteSpace(username))
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - null or whitespace user", name, username, password);
                return RegisterError.InvalidUsername;
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - null or whitespace pass", name, username, password);
                return RegisterError.InvalidPassword;
            }

            if (name.Length < 2 || name.Length > 20)
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - out of bounds username", name, username, password);
                return RegisterError.InvalidName;
            }

            if (username.Length < 2 || username.Length > 20)
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - out of bounds user", name, username, password);
                return RegisterError.InvalidUsername;
            }

            if (password.Length < 2 || password.Length > 40)
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - out of bounds pass", name, username, password);
                return RegisterError.InvalidPassword;
            }

            User user;
            if (Users.TryGetValue(username, out user))
            {
                Logger.Log("Register", "fail: name={0} username={1} password={2} - user exists", name, username, password);
                return RegisterError.ExistingUsername;
            }

            user = new User(name, username, password);

            Users.TryAdd(username, user);

            if (!_applyingLogs)
                _actionLog.LogAction(new NewUserAction { Name = username, User = username, Password = password });

            Logger.Log("Register", "name={0} username={1} password={2}", name, username, password);

            return RegisterError.None;
        }

        public LoginError Login(String username, String password, out User user)
        {
            user = null;
            Logger.Log("Login", "attempt: username={0} password={1}", username, password);

            if (String.IsNullOrWhiteSpace(username))
            {
                Logger.Log("Login", "fail: username={0} password={1} - null or whitespace user", username, password);
                return LoginError.UnexistingUser;;
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                Logger.Log("Login", "fail: username={0} password={1} - null or whitespace pass", username, password);
                return LoginError.InvalidPassword;
            }

            if (!Users.TryGetValue(username, out user))
            {
                Logger.Log("Login", "fail: username={0} password={1} - doesn't exist", username, password);
                return LoginError.UnexistingUser;
            }

            if (user.PasswordHash != Utilities.ComputeHash(password))
            {
                Logger.Log("Login", "fail: username={0} password={1} - wrong pass", username, password);
                return LoginError.InvalidPassword;
            }

            if (user.LoggedIn)
            {
                Logger.Log("Login", "fail: username={0} password={1} - already logged in", username, password);
                return LoginError.AlreadyLoggedIn;
            }

            user.LoggedIn = true;

            Logger.Log("Login", "username={0} password={1}", username, password);

            return LoginError.None;
        }

        public LogoutError Logout(String username, String password)
        {
            Logger.Log("Logout", "attempt: username={0} password={1}", username, password);

            User user;
            if (!Users.TryGetValue(username, out user))
            {
                Logger.Log("Logout", "fail: username={0} password={1} - doesn't exist", username, password);
                return LogoutError.UnexistingUser;
            }

            if (user.PasswordHash != Utilities.ComputeHash(password))
            {
                Logger.Log("Logout", "fail: username={0} password={1} - invalid pass", username, password);
                return LogoutError.InvalidPassword;
            }

            if (!user.LoggedIn)
            {
                Logger.Log("Logout", "fail: username={0} password={1} - not logged in", username, password);
                return LogoutError.NotLoggedIn;
            }

            user.LoggedIn = false;

            Logger.Log("Logout", "username={0} password={1}", username, password);

            return LogoutError.None;
        }

        public IEnumerable<Diginote> GetDiginotes(String username, String password)
        {
            // insert login logic here
            return Users[username].Diginotes;
        }

        public List<PurchaseOrder> GetPurchaseOrders(String username, String password)
        {
            // insert login logic here
            var purchaseOrders = PurchaseOrders.Where((p) => p.Buyer == Users[username]).ToList();
            return purchaseOrders;
        }

        public List<SalesOrder> GetSalesOrders(String username, String password)
        {
            // insert login logic here
            var salesOrders = SalesOrders.Where((s) => s.Seller == Users[username]).ToList();
            return salesOrders;
        } 

        public PurchaseResult CreatePurchaseOrder(String username, String password, int quantity)
        {
            Logger.Log("CreatePurchaseOrder", "attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            // insert login logic here

            var requestingUser = Users[username];

            // get available offers
            var numOffers = SalesOrders.Where(order => !order.Fulfilled).Sum(order => order.Count);

            if (numOffers == 0)
            {
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, Quotation));
                PublishMessage(Update.General);
                return PurchaseResult.Unfulfilled;
            }

            var surplus = quantity - numOffers;

            var availableDiginotes =
                (from salesOrder in SalesOrders
                 join user in Users.Values on salesOrder.Seller.Username equals user.Username
                 select user)
                    .SelectMany(user => user.Diginotes.ToList());

            // purchase order is totally fulfilled
            if (surplus <= 0)
            {

                // transfer diginotes
                var chosenDiginotes = availableDiginotes.Take(quantity);

                foreach (var chosenDiginote in chosenDiginotes)
                {
                    requestingUser.AddDiginote(chosenDiginote);
                    var diginote = chosenDiginote;
                    Users.Values.Where(u => u.Diginotes.Contains(diginote)).Select(u => u.RemoveDiginote(diginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, Quotation, true));
                PublishMessage(Update.General);
                return PurchaseResult.Fulfilled;
            }
            else // the order is partially fulfilled
            {
                // transfer diginotes
                var chosenDiginotes = availableDiginotes.Take(numOffers);

                foreach (var chosenDiginote in chosenDiginotes)
                {
                    requestingUser.AddDiginote(chosenDiginote);
                    var diginote = chosenDiginote;
                    Users.Values.Where(u => u.Diginotes.Contains(diginote)).Select(u => u.RemoveDiginote(diginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, numOffers, Quotation, true)); // fulfilled
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, surplus, Quotation)); // unfulfiled
                PublishMessage(Update.General);
                return PurchaseResult.PartiallyFullfilled;
            }
        }

        public bool UpdatePurchaseOrder(string username, string password, int id, decimal value)
        {
            // insert login logic
            var purchaseOrder = PurchaseOrders.Where((order) => order.Id == id && order.Buyer == Users[username]).ToList().FirstOrDefault();

            if (purchaseOrder == null)
                return false;

            purchaseOrder.Value = value;
            PublishMessage(Update.General);
            return true;
        }

        public void DeletePurchaseOrder(string username, string password, int id)
        {
            // insert login logic
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

        public SalesResult CreateSalesOrder(string username, string password, int quantity)
        {
            Logger.Log("CreateSalesOrder", "attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            // insert login logic here
            var requestingUser = Users[username];

            // get available offers
            var availablePurchaseOrders = PurchaseOrders.Where(order => !order.FulFilled);
            var numOffers = availablePurchaseOrders.Sum(order => order.Count);

            if (numOffers == 0)
            {
                SalesOrders.Add(new SalesOrder(requestingUser, quantity, Quotation));
                return SalesResult.Unfulfilled;
            }

            var surplus = quantity - numOffers;
            var selectedPurchaseOrders = new List<PurchaseOrder>();
            for (int i = 0, purchaseQuantity = 0; i < availablePurchaseOrders.Count() || purchaseQuantity < quantity; ++i)
            {
                //if ()
                selectedPurchaseOrders.Add(availablePurchaseOrders.ElementAt(i));

                purchaseQuantity += availablePurchaseOrders.ElementAt(i).Count;
            }

            // purchase order is totally fulfilled
            /*if (surplus <= 0)
            {

                // transfer diginotes
                var chosenDiginotes = availableDiginotes.Take(quantity);

                foreach (var chosenDiginote in chosenDiginotes)
                {
                    requestingUser.AddDiginote(chosenDiginote);
                    Users.Values.Where((User u) => u.Diginotes.Contains(chosenDiginote)).Select((User u) => u.RemoveDiginote(chosenDiginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, true));
                return SalesResult.Fulfilled;
            }
            else // the order is partially fulfilled
            {
                // transfer diginotes
                var chosenDiginotes = availableDiginotes.Take(numOffers);

                foreach (var chosenDiginote in chosenDiginotes)
                {
                    requestingUser.AddDiginote(chosenDiginote);
                    Users.Values.Where((User u) => u.Diginotes.Contains(chosenDiginote)).Select((User u) => u.RemoveDiginote(chosenDiginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, numOffers, true)); // fulfilled
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, surplus, false)); // unfulfiled
                return SalesResult.PartiallyFullfilled;
            } */

            return SalesResult.Fulfilled;
        }
    }
}
