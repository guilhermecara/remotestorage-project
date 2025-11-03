namespace remotestorage_api.Services
{
    using BCrypt.Net;
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class PasswordService
    {
        public static string Sha256HashString(string input)
        {
            byte[] shaHash = ComputeSHA256Hash(input);
            string shaHashString = BitConverter.ToString(shaHash).Replace("-", "").ToLowerInvariant();

            return shaHashString;
        }
        public static string BCryptHashString (string input)
        {
            string hashedPassword = BCrypt.HashPassword(input, workFactor: 12);
            return hashedPassword;
        }
        public static string HashPassword(string input) // Two-Step hashing. Hash to SHA 256 and then hash with BCrypt
        {
            string shaHashedPassword = Sha256HashString(input);
            string bcryptHashedPassword = BCryptHashString(shaHashedPassword);

            return bcryptHashedPassword;
        }
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has occurred: " + ex);
                return false;
            }
        }

        private static byte[] ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }
    }
    
    
}