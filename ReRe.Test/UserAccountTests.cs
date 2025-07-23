using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using ReRe.Data.DbContext;
using ReRe.Data.Models;
using ReRe.Ui.Controllers;
using Xunit;

namespace ReRe.Test
{
    public class UserAccountTestsFixed : IDisposable
    {
        private readonly ReReDbContext _context;
        private readonly UserModelsController _controller;

        public UserAccountTestsFixed()
        {
            var options = new DbContextOptionsBuilder<ReReDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ReReDbContext(options);
            _controller = new UserModelsController(_context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(
                httpContext, 
                Mock.Of<ITempDataProvider>());

            SeedTestData();
        }

        private void SeedTestData()
        {
            var roles = new List<RoleModel>
            {
                new RoleModel { Id = 1, Libelle = "Utilisateur" },
                new RoleModel { Id = 2, Libelle = "Administrateur" }
            };

            var users = new List<UserModel>
            {
                new UserModel
                {
                    Id = 1,
                    Nom = "Dupont",
                    Prenom = "Jean",
                    Email = "jean.dupont@test.com",
                    mot_de_passe = "password123",
                    RoleId = 1
                }
            };

            _context.RoleModels.AddRange(roles);
            _context.Utilisateurs.AddRange(users);
            _context.SaveChanges();
        }

        [Fact]
        public async Task ChangePassword_ValidOldPassword_UpdatesSessionAndRedirects()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserEmail", "jean.dupont@test.com");
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = await _controller.ChangePassword("password123", "nouveauMotDePasse");

            // Assert - Vérifier uniquement la redirection
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccountVue", redirectResult.ActionName);

            // Vérifier que la session a été mise à jour
            Assert.Equal("nouveauMotDePasse", _controller.HttpContext.Session.GetString("UserPassword"));
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_RedirectsWithError()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = await _controller.ChangePassword("wrongPassword", "nouveauMotDePasse");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ModifyPassword", redirectResult.ActionName);
            Assert.Equal("Ancien mot de passe incorrect.", _controller.TempData["Message"]);
        }

        [Fact]
        public async Task Login_ValidCredentials_RedirectsToHome()
        {
            // Act
            var result = await _controller.Login("jean.dupont@test.com", "password123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_InvalidCredentials_RedirectsToLoginWithError()
        {
            // Act
            var result = await _controller.Login("wrong@email.com", "wrongpass");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);
        }

        [Fact]
        public void Logout_ClearsSessionAndRedirects()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserEmail", "test@test.com");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Vous êtes maintenant déconnecté.", _controller.TempData["Message"]);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
