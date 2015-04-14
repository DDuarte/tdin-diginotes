using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Session
    {
        public string Username;
        public string Password;

        public Session(string username, string password) 
        {
            Username = username;
            Password = password;
        }
    }
}
