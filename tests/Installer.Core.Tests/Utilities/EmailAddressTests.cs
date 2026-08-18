using Installer.Core.Utilities;

namespace Installer.Core.Tests.Utilities;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("matt.brossard323@gmail.com")]
    [InlineData("  Name@Email.com  ")]
    public void TryNormalize_accepts_simple_addresses(string value)
    {
        Assert.True(EmailAddress.TryNormalize(value, out var email));
        Assert.Equal(value.Trim(), email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@missing.local")]
    [InlineData("missing@")]
    public void TryNormalize_rejects_invalid(string value) =>
        Assert.False(EmailAddress.TryNormalize(value, out _));

    [Fact]
    public void IsPlaceholder_detects_example_com()
    {
        Assert.True(EmailAddress.IsPlaceholder("support@example.com"));
        Assert.False(EmailAddress.IsPlaceholder("matt.brossard323@gmail.com"));
    }

    [Fact]
    public void Default_prefers_last_saved_then_manifest_then_publisher()
    {
        Assert.Equal(
            "saved@test.com",
            EmailAddress.Default("saved@test.com", "manifest@test.com", "publisher@test.com"));
        Assert.Equal(
            "manifest@test.com",
            EmailAddress.Default("support@example.com", "manifest@test.com", "publisher@test.com"));
        Assert.Equal(
            "publisher@test.com",
            EmailAddress.Default(null, "support@example.com", "publisher@test.com"));
        Assert.Equal("", EmailAddress.Default(null, "support@example.com", "bad"));
    }
}
