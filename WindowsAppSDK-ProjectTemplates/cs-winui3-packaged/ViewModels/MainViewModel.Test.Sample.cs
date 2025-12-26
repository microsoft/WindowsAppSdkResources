using BlankApp.Models;
using BlankApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BlankApp.ViewModels.Tests;

/// <summary>
/// Unit tests for MainViewModel.
/// Demonstrates ViewModel testing with mocked services.
/// </summary>
[TestClass]
public class MainViewModelTests
{
    private Mock<ILogger<MainViewModel>> _mockLogger = null!;
    private Mock<IUserService> _mockUserService = null!;
    private MainViewModel _sut = null!; // System Under Test

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<MainViewModel>>();
        _mockUserService = new Mock<IUserService>();
        _sut = new MainViewModel(_mockLogger.Object, _mockUserService.Object);
    }

    [TestMethod]
    public async Task LoadDataAsync_WhenSuccessful_ShouldUpdateTitle()
    {
        // Arrange
        var expectedUser = new User { Id = 1, Name = "John Doe", Age = 30 };
        _mockUserService.Setup(s => s.GetUserAsync(1))
            .ReturnsAsync(expectedUser);

        // Act
        await _sut.LoadDataAsync();

        // Assert
        Assert.AreEqual("Welcome, John Doe!", _sut.Title);
        Assert.AreEqual(expectedUser, _sut.CurrentUser);
        Assert.IsFalse(_sut.IsLoading);
        _mockUserService.Verify(s => s.GetUserAsync(1), Times.Once);
    }

    [TestMethod]
    public async Task LoadDataAsync_WhenUserNotFound_ShouldUpdateStatusMessage()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserAsync(1))
            .ReturnsAsync((User?)null);

        // Act
        await _sut.LoadDataAsync();

        // Assert
        Assert.IsNull(_sut.CurrentUser);
        Assert.AreEqual("User not found", _sut.StatusMessage);
        Assert.IsFalse(_sut.IsLoading);
    }

    [TestMethod]
    public async Task LoadDataAsync_WhenServiceFails_ShouldHandleError()
    {
        // Arrange
        _mockUserService.Setup(s => s.GetUserAsync(1))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        await _sut.LoadDataAsync();

        // Assert
        Assert.AreEqual("Error loading data", _sut.StatusMessage);
        Assert.IsFalse(_sut.IsLoading);
    }

    [TestMethod]
    public void Title_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        _sut.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_sut.Title))
                propertyChangedRaised = true;
        };

        // Act
        _sut.Title = "New Title";

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual("New Title", _sut.Title);
    }
}
