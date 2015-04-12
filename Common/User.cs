using System;
using System.Collections.Generic;
using OpenNETCF.ORM;

namespace Common
{
    [Entity(KeyScheme.Identity, NameInStore = "Usr")]
    public class User : MarshalByRefObject
    {
        [Field(IsPrimaryKey = true)]
        public Guid Id { get; private set; }

        [Field(RequireUniqueValue = true)]
        public String Username { get; private set; }

        [Field]
        public String PasswordHash { get; private set; }

        [Reference(typeof(Diginote), "Serial")]
        public HashSet<Diginote> Diginotes { get; private set; }

        [Field]
        public bool LoggedIn { get; set; }

        public User(String username, String password)
        {
            Id = Guid.NewGuid();
            Username = username;
            PasswordHash = Utilities.ComputeHash(password);
            Diginotes = new HashSet<Diginote>();
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
