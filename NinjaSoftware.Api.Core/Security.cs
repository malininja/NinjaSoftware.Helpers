using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Configuration;

namespace NinjaSoftware.Api.Core
{
    public static class Security
    {
        public static string GetPasswordHash(string password)
        {
            string salt = ConfigurationManager.AppSettings["PasswordSalt"];

            MD5CryptoServiceProvider cryptoProvider = new MD5CryptoServiceProvider();

            byte[] bytes = Encoding.UTF8.GetBytes(string.Format("{0}{1}", salt, password));
            byte[] hashedBytes = cryptoProvider.ComputeHash(bytes);

            return Convert.ToBase64String(hashedBytes);
        }
    }
}
