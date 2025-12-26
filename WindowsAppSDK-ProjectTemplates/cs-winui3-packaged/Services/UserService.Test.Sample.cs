using BlankApp.Data;
using BlankApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlankApp.Services.Tests;

/// <summary>
/// Unit tests for UserService class.
/// Test file is co-located with source: UserService.Test.cs beside UserService.cs
/// </summary>
[TestClass]
public class UserServiceTests
{
    private Mock<ILogger<UserService>> _mockLogger = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private UserService _sut = null!; // System Under Test

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UserService>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _sut = new UserService(_mockLogger.Object, _mockUserRepository.Object);
    }

    [TestMethod]
    public async Task GetUserAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "John Doe" };
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        Assert.AreEqual("John Doe", result.Name);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetUserAsync_WhenUserNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserAsync(userId);

        // Assert
        Assert.IsNull(result);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task GetUserAsync_WhenInvalidUserId_ShouldThrowArgumentException(int userId)
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _sut.GetUserAsync(userId));
        _mockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }
}
