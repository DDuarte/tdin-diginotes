using System;
using System.Collections.Generic;

namespace Common
{
    public class User : MarshalByRefObject
    {
        public string Name { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public HashSet<Diginote> Diginotes { get; private set; }
        public decimal Balance { get; private set; }
        public bool LoggedIn { get; set; }

        public User(string name, string username, string password)
        {
            Name = name;
            Username = username;
            PasswordHash = Utilities.ComputeHash(password);
            Diginotes = new HashSet<Diginote>();
            Balance = 0;
        }

        public bool AddDiginote(Diginote diginote) 
        {
            return Diginotes.Add(diginote);
        }

        public bool RemoveDiginote(Diginote diginote)
        {
            return Diginotes.Remove(diginote);
        }

        public void AddFunds(decimal euros)
        {
            Balance += euros;
        }

        public override object InitializeLifetimeService()
        {
            return null; // infinite
        }
    }
}
