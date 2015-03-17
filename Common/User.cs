using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

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
            var encrypt = SHA256.Create();
            Username = username;
            PasswordHash = Encoding.UTF8.GetString(encrypt.ComputeHash(Encoding.UTF8.GetBytes(password)));
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
