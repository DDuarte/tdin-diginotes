using System;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class Utilities
    {
        public static String ComputeHash(String val)
        {
            var encrypt = SHA256.Create();
            return Encoding.UTF8.GetString(encrypt.ComputeHash(Encoding.UTF8.GetBytes(val)));
        }
    }
}
