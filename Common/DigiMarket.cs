using System;
using System.Collections.Generic;

namespace Common
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
        public readonly Dictionary<String, User> Users = new Dictionary<string, User>();
        public readonly List<PurchaseOrder> PurchaseOrders = new List<PurchaseOrder>();
        public readonly List<SalesOrder> SalesOrders = new List<SalesOrder>();

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }

        public RegisterError Register(String username, String password)
        {
            if (String.IsNullOrWhiteSpace(username))
                return RegisterError.InvalidUsername;

            if (String.IsNullOrWhiteSpace(password))
                return RegisterError.InvalidPassword;

            if (username.Length < 2 || username.Length > 20)
                return RegisterError.InvalidUsername;

            if (password.Length < 2 || password.Length > 40)
                return RegisterError.InvalidPassword;

            User user;
            if (Users.TryGetValue(username, out user))
                return RegisterError.ExistingUsername;

            user = new User(username, password);

            Users.Add(username, user);
            
            return RegisterError.None;
        }

        public LoginError Login(String username, String password)
        {
            Console.WriteLine("Login from {0}!", username);

            User user;
            if (!Users.TryGetValue(username, out user))
                return LoginError.UnexistingUser;

            if (user.PasswordHash != Utilities.ComputeHash(password))
                return LoginError.InvalidPassword;

            if (user.LoggedIn)
                return LoginError.AlreadyLoggedIn;

            user.LoggedIn = true;

            return LoginError.None;
        }

        public LogoutError Logout(String username, String password)
        {
            User user;
            if (!Users.TryGetValue(username, out user))
                return LogoutError.UnexistingUser;

            if (user.PasswordHash != Utilities.ComputeHash(password))
                return LogoutError.InvalidPassword;

            if (!user.LoggedIn)
                return LogoutError.NotLoggedIn;

            user.LoggedIn = false;

            return LogoutError.None;   
        }
    }
}
