using System.Security.Cryptography;

namespace PackageManager.Core;

public static class HashValidator
{
    public static async Task<string> CalculateSha256Async(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static bool Validate(string filePath, string expectedHash)
    {
        var actualHash = CalculateSha256Async(filePath).GetAwaiter().GetResult();
        return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
    }
}

public class Downloader
{
    private readonly HttpClient _httpClient = new();

    public async Task DownloadFileAsync(string url, string destinationPath, Action<long, long?>? onProgress = null)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength;
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long totalRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) != 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;
            onProgress?.Invoke(totalRead, totalBytes);
        }
    }
}

public static class PackageIdParser
{
    public static (string Namespace, string Name, string? Version) Parse(string packageId)
    {
        // Format: @user/package:version or @user/package
        string ns = "";
        string name = "";
        string? version = null;

        var parts = packageId.Split(':');
        if (parts.Length > 1)
        {
            version = parts[1];
        }

        var pathParts = parts[0].Split('/');
        if (pathParts.Length > 1)
        {
            ns = pathParts[0];
            name = pathParts[1];
        }
        else
        {
            name = pathParts[0];
        }

        return (ns, name, version);
    }
}
