using Xunit;
using PackageManager.Core;
using System.IO;

namespace PackageManager.Tests;

public class CoreTests
{
    [Theory]
    [InlineData("@user/package:1.0.0", "@user", "package", "1.0.0")]
    [InlineData("@user/package", "@user", "package", null)]
    [InlineData("simple-pkg", "", "simple-pkg", null)]
    public void PackageIdParser_ShouldParseCorrectly(string input, string expectedNs, string expectedName, string? expectedVer)
    {
        var (ns, name, ver) = PackageIdParser.Parse(input);
        
        Assert.Equal(expectedNs, ns);
        Assert.Equal(expectedName, name);
        Assert.Equal(expectedVer, ver);
    }

    [Fact]
    public async Task HashValidator_ShouldCalculateCorrectHash()
    {
        // Create a temp file
        var path = Path.GetTempFileName();
        await File.WriteAllTextAsync(path, "hello world");
        
        // SHA256 of "hello world" is b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9
        var expected = "b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9";
        
        var actual = await HashValidator.CalculateSha256Async(path);
        
        Assert.Equal(expected, actual);
        
        File.Delete(path);
    }
}
