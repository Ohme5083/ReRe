using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReRe.Data.DbContext;
using ReRe.Data.Models;
using ReRe.Ui.Controllers;

namespace ReRe.Test;
public class UserModelsControllerTests
{
    private readonly Mock<DbSet<UserModel>> _userSetMock;
    private readonly Mock<ReReDbContext> _dbContextMock;
    private readonly UserModelsController _controller;

    public UserModelsControllerTests()
    {
        var users = new List<UserModel>
        {
            new UserModel
            {
                Id = 1,
                Email = "test@ex.com",
                mot_de_passe = "1234",
                Nom = "Test",
                Prenom = "User",
                RoleId = 2,
                Role = new RoleModel { Id = 2, Libelle = "Admin" }
            }
        }.AsQueryable();

        _userSetMock = new Mock<DbSet<UserModel>>();
        _userSetMock.As<IQueryable<UserModel>>().Setup(m => m.Provider).Returns(users.Provider);
        _userSetMock.As<IQueryable<UserModel>>().Setup(m => m.Expression).Returns(users.Expression);
        _userSetMock.As<IQueryable<UserModel>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _userSetMock.As<IQueryable<UserModel>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbContextMock = new Mock<ReReDbContext>();
        _dbContextMock.Setup(db => db.Utilisateurs).Returns(_userSetMock.Object);

        _controller = new UserModelsController(_dbContextMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Session = new DummySession();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Login_ValidCredentials_RedirectsToHomeIndex()
    {
        // Act
        var result = await _controller.Login("test@ex.com", "1234");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);

        // Session checks (optional)
        Assert.Equal("test@ex.com", _controller.HttpContext.Session.GetString("UserEmail"));
        Assert.Equal("1234", _controller.HttpContext.Session.GetString("UserPassword"));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsToLoginWithMessage()
    {
        // Act
        var result = await _controller.Login("test@ex.com", "wrongpassword");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);
    }

    [Fact]
    public async Task Login_NonExistentUser_ReturnsToLoginWithMessage()
    {
        // Act
        var result = await _controller.Login("unknown@ex.com", "1234");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);
        Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);
    }
}
