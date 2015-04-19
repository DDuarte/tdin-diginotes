using System;
using System.Collections.Generic;

namespace Common
{
    public class User : MarshalByRefObject
    {
        public string Name { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public readonly HashSet<Diginote> Diginotes = new HashSet<Diginote>();
        public bool LoggedIn { get; set; }

        public User(string name, string username, string password)
        {
            Name = name;
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
