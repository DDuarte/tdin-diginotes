using System;
using System.Collections.Generic;

namespace Common
{
    public class User : MarshalByRefObject
    {
        public String Username { get; private set; }
        public String PasswordHash { get; private set; }
        public readonly HashSet<Diginote> Diginotes = new HashSet<Diginote>();
        public bool LoggedIn { get; set; }

        public User(String username, String password)
        {
            Username = username;
            PasswordHash = Utilities.ComputeHash(password);
        }

        public bool AddDiginote(Diginote diginote) 
        {
            return Diginotes.Add(diginote);
        }

        public bool RemoveDiginote(Diginote diginote)
        {
            return Diginotes.Remove(diginote);
        }
    }
}
