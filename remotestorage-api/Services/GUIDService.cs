namespace remotestorage_api.Services;

public static class GUIDService
{
    // Example: "20251103_154501_2f5b3c6e8a574c2db0f04b7f7e1b9a2d.jpg"
    public static string GenerateTimestampedGuidString(string input)
    {
        string ext = Path.GetExtension(input) ?? "";
        string ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string guidCompact = Guid.NewGuid().ToString("N"); // no dashes
        return $"{ts}_{guidCompact}{ext}";
    }

    public static string GenerateGuidString ()
    {
        string randomGuid = $"{Guid.NewGuid()}";
        return randomGuid;
    }
}