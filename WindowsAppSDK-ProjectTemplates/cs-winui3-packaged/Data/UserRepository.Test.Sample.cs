using BlankApp.Data;
using BlankApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlankApp.Data.Tests;

/// <summary>
/// Unit tests for UserRepository.
/// Demonstrates data layer testing with proper mocking.
/// </summary>
[TestClass]
public class UserRepositoryTests
{
    private Mock<ILogger<UserRepository>> _mockLogger = null!;
    private UserRepository _sut = null!; // System Under Test

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _sut = new UserRepository(_mockLogger.Object);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com", Age = 30 };
        await _sut.CreateAsync(user);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
        Assert.AreEqual("John Doe", result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateAsync_WhenValidUser_ShouldAssignIdAndReturnIt()
    {
        // Arrange
        var user = new User { Name = "Jane Doe", Email = "jane@example.com", Age = 25 };

        // Act
        var userId = await _sut.CreateAsync(user);

        // Assert
        Assert.IsTrue(userId > 0);
        Assert.AreEqual(userId, user.Id);
        Assert.IsTrue(user.CreatedAt > DateTime.MinValue);
    }

    [TestMethod]
    public async Task CreateAsync_WhenUserIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
    }

    [TestMethod]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new UserRepository(null!));
    }
}
