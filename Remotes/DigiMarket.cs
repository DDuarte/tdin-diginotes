using System;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace Remotes
{
    public interface IDigiMarket
    {
        RegisterError Register(String username, String password);
        LoginError Login(String username, String password);
        LogoutError Logout(String username, String password);
    }

    public class DigiMarket : MarshalByRefObject, IDigiMarket
    {
        public readonly decimal Quotation = 1;

        public readonly Dictionary<String, User> Users = new Dictionary<string, User>()
        {
            { "test", new User("test", "test") }
        };

        public readonly List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
        public readonly List<SalesOrder> SalesOrders = new List<SalesOrder>();

        public override object InitializeLifetimeService()
        {
            return null; // infinite
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

            User user;
            if (Users.TryGetValue(username, out user))
            {
                Logger.Log("Register", "fail: username={0} password={1} - user exists", username, password);
                return RegisterError.ExistingUsername;
            }

            user = new User(username, password);

            Users.Add(username, user);

            Logger.Log("Register", "username={0} password={1}", username, password);

            return RegisterError.None;
        }

        public LoginError Login(String username, String password)
        {
            Logger.Log("Login", "attempt: username={0} password={1}", username, password);

            User user;
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

        public PurchaseResult CreatePurchaseOrder(String username, String password, int quantity)
        {
            Logger.Log("CreatePurchaseOrder", "attempt: username={0} password={1} quantity={2}",
                username, password, quantity);

            // get available offers
            var numOffers = SalesOrders.Sum((SalesOrder order) => order.Count);
            var surplus = numOffers - quantity;

            var availableDiginotes =
                (from salesOrder in SalesOrders
                    join user in Users.Values on salesOrder.Seller.Username equals user.Username
                    select user)
                    .SelectMany(user => user.Diginotes.ToList());

            return PurchaseResult.Fulfilled;
        }
    }
}
