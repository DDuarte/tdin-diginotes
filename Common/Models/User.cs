using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class User
    {
        private static int _last = 1;
        public int UserId { get; private set; }
        public string Username { get; private set; }
        public string Name { get; private set; }
        public string PasswordHash { get; private set; }
        public List<Diginote> Diginotes { get; set; }
        public decimal Balance { get; private set; }
        public bool LoggedIn { get; private set; }

        public User(string name, string username, string password)
        {
            UserId = _last++;
            Name = name;
            Username = username;
            PasswordHash = Utilities.ComputeHash(password);
            Diginotes = new List<Diginote>();
            Balance = 0;
        }

        protected bool Equals(User other)
        {
            return UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return UserId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
        }

        public static bool operator ==(User left, User right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(User left, User right)
        {
            return !Equals(left, right);
        }
    }
}
