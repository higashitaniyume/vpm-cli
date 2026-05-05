using System.Text.Json.Serialization;

namespace PackageManager.Core.Models;

public record ApiResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("message")] string? Message = null
);

public record Package(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("latest_version")] string? LatestVersion,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt
);

public record PackageDetail(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("versions")] List<PackageVersionInfo> Versions
);

public record PackageVersionInfo(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("release_notes")] string? ReleaseNotes,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
);

public record PackageVersion(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("package_id")] string PackageId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("installer_url")] string InstallerUrl,
    [property: JsonPropertyName("file_hash")] string FileHash,
    [property: JsonPropertyName("release_notes")] string? ReleaseNotes,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
);

public record PublishPackageRequest(
    [property: JsonPropertyName("namespace")] string Namespace,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("installer_url")] string InstallerUrl,
    [property: JsonPropertyName("file_hash")] string FileHash,
    [property: JsonPropertyName("release_notes")] string? ReleaseNotes
);
