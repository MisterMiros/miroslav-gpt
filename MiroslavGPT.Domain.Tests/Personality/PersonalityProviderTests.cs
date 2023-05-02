using MiroslavGPT.Domain.Personality;

namespace MiroslavGPT.Domain.Tests.Personality;

[TestFixture]
public class PersonalityProviderTests
{
    private PersonalityProvider _provider;

    [SetUp]
    public void SetUp()
    {
        _provider = new PersonalityProvider();
    }

    [TestCase("/prompt")]
    [TestCase("/dan")]
    [TestCase("/tsundere")]
    [TestCase("/bravo")]
    [TestCase("/inverse")]
    public void HasPersonalityCommand_ShouldReturnTrue_WithExistingPersonality(string command)
    {
        // Arrange
        // Act
        var result = _provider.HasPersonalityCommand(command);

        // Assert
        result.Should().BeTrue();
    }

    [TestCase("/command")]
    [TestCase("ABCDEF")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void HasPersonalityCommand_ShouldReturnFalse_WithNotExistingPersonality(string command)
    {
        // Arrange
        // Act
        var result = _provider.HasPersonalityCommand(command);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase("/dan")]
    [TestCase("/tsundere")]
    [TestCase("/bravo")]
    [TestCase("/inverse")]
    public void GetPersonalityMessages_ShouldReturnMessages_WithExistingPersonality(string command)
    {
        // Arrange
        // Act
        var result = _provider.GetPersonalityMessages(command);

        // Assert
        result.Should().NotBeNull().And.NotBeEmpty();
    }

    [TestCase("/prompt")]
    [TestCase("/command")]
    public void GetPersonalityMessages_ShouldReturnEmptyList_WithDefaultOrNonExistingPersonality(string command)
    {
        // Arrange
        // Act
        var result = _provider.GetPersonalityMessages(command);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }
}