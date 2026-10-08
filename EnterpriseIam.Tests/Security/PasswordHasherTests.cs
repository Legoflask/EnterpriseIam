using EnterpriseIam.Infrastructure.Security;
using Xunit;

namespace EnterpriseIam.Tests.Security;

public class PasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact] // 🚀 Defines a single unique unit test case
    public void HashPassword_ShouldReturnValidHashString()
    {
        // Arrange (Setup your test variables)
        var clearTextPassword = "SuperSecurePassword123!";

        // Act (Execute the target logic)
        var generatedHash = _hasher.HashPassword(clearTextPassword);

        // Assert (Verify the output matches engineering requirements)
        Assert.NotNull(generatedHash);
        Assert.NotEmpty(generatedHash);
        Assert.NotEqual(clearTextPassword, generatedHash); // It should never match plain text
    }

    [Theory] // 🚀 Allows running the exact same test multiple times with varying parameters
    [InlineData("PasswordA!123")]
    [InlineData("AnotherSecret_99")]
    [InlineData("Short1!")]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash(string password)
    {
        // Arrange
        var hash = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var realPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _hasher.HashPassword(realPassword);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }
}