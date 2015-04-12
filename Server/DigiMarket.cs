using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using OpenNETCF.ORM;
using Remotes;

namespace Server
{
    // ReSharper disable once UnusedMember.Global
    [Entity(KeyScheme.Identity)]
    public class DigiMarket : MarshalByRefObject, IDigiMarket
    {
        [Field(IsPrimaryKey = true)]
        public int Id { get; private set; }

        // [Field]
        public decimal Quotation { get; private set; }

        [Reference(typeof(User), "Id")]
        public HashSet<User> Users { get; private set; }

        [Reference(typeof(PurchaseOrder), "Id")]
        public HashSet<PurchaseOrder> PurchaseOrders { get; private set; }

        [Reference(typeof(SalesOrder), "Id")]
        public HashSet<SalesOrder> SalesOrders { get; private set; }

        public readonly IDataStore Store = DataStoreHelper.GetDataStore();

        public DigiMarket()
        {
            var markets = Store.Select<DigiMarket>();
            if (!markets.Any())
            {
                Quotation = 1;
                Users = new HashSet<User>();
                PurchaseOrders = new HashSet<PurchaseOrder>();
                SalesOrders = new HashSet<SalesOrder>();
                Store.Insert(this);
            }
            else
            {
                var market = markets.First();
                Quotation = market.Quotation;
                Users = market.Users;
                PurchaseOrders = market.PurchaseOrders;
                SalesOrders = market.SalesOrders;
            }
        }

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }

        private User FindUser(string username)
        {
            return Users.FirstOrDefault(user => user.Username == username);
        }

        public RegisterError Register(String username, String password)
        {
            Logger.Log("Register", "attempt: username={0} password={1}", username, password);

            if (String.IsNullOrWhiteSpace(username))
            {
                Logger.Log("Register", "fail: username={0} password={1} - null or whitespace user", username, password);
                return RegisterError.InvalidUsername;
            }

            if (String.IsNullOrWhiteSpace(password))
            {
                Logger.Log("Register", "fail: username={0} password={1} - null or whitespace pass", username, password);
                return RegisterError.InvalidPassword;
            }

            if (username.Length < 2 || username.Length > 20)
            {
                Logger.Log("Register", "fail: username={0} password={1} - out of bounds user", username, password);
                return RegisterError.InvalidUsername;
            }

            if (password.Length < 2 || password.Length > 40)
            {
                Logger.Log("Register", "fail: username={0} password={1} - out of bounds pass", username, password);
                return RegisterError.InvalidPassword;
            }

            var user = FindUser(username);
            if (user != null)
            {
                Logger.Log("Register", "fail: username={0} password={1} - user exists", username, password);
                return RegisterError.ExistingUsername;
            }

            user = new User(username, password);

            Users.Add(user);
            Store.Insert(user);

            Logger.Log("Register", "username={0} password={1}", username, password);

            return RegisterError.None;
        }

        public LoginError Login(String username, String password)
        {
            Logger.Log("Login", "attempt: username={0} password={1}", username, password);

            var user = FindUser(username);
            if (user == null)
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

            var user = FindUser(username);
            if (user == null)
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

        public PurchaseResult CreatePurchaseOrder(String username, String password, int quantity)
        {
            Logger.Log("CreatePurchaseOrder", "attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            // insert login logic here

            var requestingUser = FindUser(username);

            // get available offers
            var numOffers = SalesOrders.Where((SalesOrder order) => !order.Fulfilled).Sum((SalesOrder order) => order.Count);

            if (numOffers == 0)
            {
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, false));
                return PurchaseResult.Unfulfilled;
            }

            var surplus = quantity - numOffers;

            var availableDiginotes =
                (from salesOrder in SalesOrders
                 join user in Users on salesOrder.Seller.Username equals user.Username
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
                    Users.Where((User u) => u.Diginotes.Contains(chosenDiginote)).Select((User u) => u.RemoveDiginote(chosenDiginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, quantity, true));
                return PurchaseResult.Fulfilled;
            }
            else // the order is partially fulfilled
            {
                // transfer diginotes
                var chosenDiginotes = availableDiginotes.Take(numOffers);

                foreach (var chosenDiginote in chosenDiginotes)
                {
                    requestingUser.AddDiginote(chosenDiginote);
                    Users.Where((User u) => u.Diginotes.Contains(chosenDiginote)).Select((User u) => u.RemoveDiginote(chosenDiginote));
                }

                PurchaseOrders.Add(new PurchaseOrder(requestingUser, numOffers, true)); // fulfilled
                PurchaseOrders.Add(new PurchaseOrder(requestingUser, surplus, false)); // unfulfiled
                return PurchaseResult.PartiallyFullfilled;
            }
        }

        public SalesResult CreateSalesOrder(string username, string password, int quantity)
        {
            Logger.Log("CreateSalesOrder", "attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            // insert login logic here
            var requestingUser = FindUser(username);

            // get available offers
            var availablePurchaseOrders = PurchaseOrders.Where((PurchaseOrder order) => !order.FulFilled);
            var numOffers = availablePurchaseOrders.Sum((PurchaseOrder order) => order.Count);

            if (numOffers == 0)
            {
                SalesOrders.Add(new SalesOrder(requestingUser, quantity, false));
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
