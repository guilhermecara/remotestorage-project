namespace remotestorage.Services;

public class UsernameService
{
    private static bool IsMinimumLength (string input, int minimumLength)
    {
        if (input.Length < minimumLength)
        {
            return false;
        }

        return true;
    }
    public static string ValidateUsername (string username)
    {
        if (!IsMinimumLength(username, 3))
        {
            return "Username must contain 3 or more characters";
        }

        return "VALID";
    }
}