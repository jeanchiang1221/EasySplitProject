using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EasySplitProject.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class HashPassword
    {
        /// <summary>
        /// 產生Salt
        /// </summary>
        /// <returns>Salt</returns>
        public static string CreateSalt()
        {
            var rng = new RNGCryptoServiceProvider();
            var buffer = new byte[5];
            rng.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        /// <summary>
        /// Computes a salted hash of the password and salt provided and returns as a base64 encoded string.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">The salt to use in the hash.</param>
        public static string GenerateHashWithSalt(string password, string salt)
        {
            // merge password and salt together
            var sHashWithSalt = salt + password + salt;
            // convert this merged value to a byte array
            var saltedHashBytes = Encoding.UTF8.GetBytes(sHashWithSalt);
            // use hash algorithm to compute the hash
            HashAlgorithm algorithm = new SHA256Managed();
            // convert merged bytes to a hash as byte array
            var hash = algorithm.ComputeHash(saltedHashBytes);
            // return the has as a base 64 encoded string
            return Convert.ToBase64String(hash);
        }

    }
}