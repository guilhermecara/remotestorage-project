namespace remotestorage.Services;

public class PasswordService
{
    private static bool IsMinimumLength (string input, int minimumLength)
    {
        if (input.Length < minimumLength)
        {
            return false;
        }

        return true;
    }
    public static string ValidatePassword (string password)
    {
        if (!IsMinimumLength(password, 8))
        {
            return "Password must contain 8 or more characters";
        }

        return "VALID";
    }
}