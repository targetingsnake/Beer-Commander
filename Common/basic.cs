using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Common.Basics
{
    internal static class Generators
    {
        public static string RandomString(int length)
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[64];
                cryptoProvider.GetBytes(bytes);
                string secureRandomString = Convert.ToBase64String(bytes);
                return secureRandomString;
            }
        }
    }
}
