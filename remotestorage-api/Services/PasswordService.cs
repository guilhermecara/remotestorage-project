

namespace remotestorage_api.Services
{
    using BCrypt.Net;

    public static class PasswordService
    {
        public static string HashPassword(string input)
        {
            string hashedPassword = BCrypt.HashPassword(input, workFactor: 12);
            return hashedPassword;
        }
        public static bool VerifyPassword (string password, string hash)
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
    }
}