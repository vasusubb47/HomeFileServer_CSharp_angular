using System.Security.Cryptography;

namespace api.UtilityClass;

public static class AppUtility
{
    public static async Task<string> CalculateSha256HashFromStreamAsync(Stream stream)
    {
        // 1. Ensure the stream is at the beginning
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var sha256 = SHA256.Create();
    
        // 2. Compute the hash
        // This reads the stream from its current position to the end
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);

        // 3. IMPORTANT: Rewind the stream! 
        // If you don't do this, the next person who tries to use this stream
        // will think the file is empty because the pointer is at the end.
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
