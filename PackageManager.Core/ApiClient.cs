using System.Net.Http.Json;
using System.Text.Json;
using PackageManager.Core.Models;

namespace PackageManager.Core;

public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
    }

    public async Task<List<Package>> SearchPackagesAsync(string query)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<Package>>>($"packages?q={Uri.EscapeDataString(query)}", _jsonOptions);
        return response?.Data ?? new List<Package>();
    }

    public async Task<PackageDetail?> GetPackageAsync(string ns, string name)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<PackageDetail>>($"packages/{ns}/{name}", _jsonOptions);
        return response?.Data;
    }

    public async Task<PackageVersion?> GetPackageVersionAsync(string ns, string name, string version)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<PackageVersion>>($"packages/{ns}/{name}/{version}", _jsonOptions);
        return response?.Data;
    }

    public async Task<ApiResponse<string>> PublishPackageAsync(PublishPackageRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("packages", request, _jsonOptions);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse<string>>(_jsonOptions);
        return content ?? new ApiResponse<string>(false, null, "Unknown error");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
