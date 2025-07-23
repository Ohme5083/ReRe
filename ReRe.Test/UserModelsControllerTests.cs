using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReRe.Data.DbContext;
using ReRe.Data.Models;
using ReRe.Ui.Controllers;
using Xunit;

namespace ReRe.Test
{
    public class UserModelsControllerTests : IDisposable
    {
        private readonly ReReDbContext _context;
        private readonly UserModelsController _controller;

        public UserModelsControllerTests()
        {
            // Créer une base de données en mémoire unique pour chaque test
            var options = new DbContextOptionsBuilder<ReReDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ReReDbContext(options);

            // Créer le contrôleur avec le contexte (comme dans votre code)
            _controller = new UserModelsController(_context);

            // Configurer le contexte HTTP pour les sessions
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new DummySession();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Initialiser la base de données
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task Login_ValidCredentials_RedirectsToHomeIndex()
        {
            // Arrange - Créer un utilisateur de test avec les bonnes propriétés
            var testUser = new UserModel
            {
                Id = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@example.com",
                mot_de_passe = "testpass",
                RoleId = 1
            };

            // Ajouter un rôle de test
            var testRole = new RoleModel
            {
                Id = 1,
                Libelle = "User"
            };

            _context.RoleModels.Add(testRole);
            _context.Utilisateurs.Add(testUser);
            await _context.SaveChangesAsync();

            // Act - Tenter de se connecter avec email et password
            var result = await _controller.Login("test@example.com", "testpass");

            // Assert - Vérifier la redirection vers Home/Index
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Vérifier que les informations sont stockées en session
            Assert.Equal("test@example.com", _controller.HttpContext.Session.GetString("UserEmail"));
            Assert.Equal("testpass", _controller.HttpContext.Session.GetString("UserPassword"));
            Assert.Equal("Dupont", _controller.HttpContext.Session.GetString("UserName"));
            Assert.Equal("Jean", _controller.HttpContext.Session.GetString("UserFirstName"));
            Assert.Equal(1, _controller.HttpContext.Session.GetInt32("Id"));
            Assert.Equal(1, _controller.HttpContext.Session.GetInt32("RoleId"));
        }

        [Fact]
        public async Task Login_InvalidPassword_RedirectsToLoginWithMessage()
        {
            // Arrange - Créer un utilisateur avec un mot de passe différent
            var testUser = new UserModel
            {
                Id = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@example.com",
                mot_de_passe = "correctpass",
                RoleId = 1
            };

            var testRole = new RoleModel
            {
                Id = 1,
                Libelle = "User"
            };

            _context.RoleModels.Add(testRole);
            _context.Utilisateurs.Add(testUser);
            await _context.SaveChangesAsync();

            // Act - Tenter de se connecter avec un mauvais mot de passe
            var result = await _controller.Login("test@example.com", "wrongpass");

            // Assert - Vérifier la redirection vers Login
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Vérifier que le message d'erreur est défini
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);

            // Vérifier qu'aucune session n'est créée
            Assert.Null(_controller.HttpContext.Session.GetString("UserEmail"));
        }

        [Fact]
        public async Task Login_NonExistentUser_RedirectsToLoginWithMessage()
        {
            // Act - Tenter de se connecter avec un utilisateur inexistant
            var result = await _controller.Login("nonexistent@example.com", "password");

            // Assert - Vérifier la redirection vers Login
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Vérifier que le message d'erreur est défini
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);

            // Vérifier qu'aucune session n'est créée
            Assert.Null(_controller.HttpContext.Session.GetString("UserEmail"));
        }

        [Fact]
        public async Task Create_ValidUser_RedirectsToLogin()
        {
            // Arrange
            var newUser = new UserModel
            {
                Nom = "Martin",
                Prenom = "Paul",
                Email = "paul.martin@example.com",
                mot_de_passe = "password123",
                RoleId = 1
            };

            // Act
            var result = await _controller.Create(newUser);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Vérifier que l'utilisateur a été ajouté
            var userInDb = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == "paul.martin@example.com");
            Assert.NotNull(userInDb);
            Assert.Equal("Martin", userInDb.Nom);
            Assert.Equal("Paul", userInDb.Prenom);
        }

        [Fact]
        public async Task Create_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange - Créer un utilisateur existant
            var existingUser = new UserModel
            {
                Id = 1,
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "test@example.com",
                mot_de_passe = "password",
                RoleId = 1
            };

            _context.Utilisateurs.Add(existingUser);
            await _context.SaveChangesAsync();

            // Tenter de créer un utilisateur avec le même email
            var newUser = new UserModel
            {
                Nom = "Martin",
                Prenom = "Paul",
                Email = "test@example.com", // Même email
                mot_de_passe = "password123",
                RoleId = 1
            };

            // Act
            var result = await _controller.Create(newUser);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Email"));
            Assert.Equal("Cet email est déjà utilisé.", _controller.ModelState["Email"]?.Errors[0].ErrorMessage);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
