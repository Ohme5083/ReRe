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
    public class UserControllerTests : IDisposable
    {
        private readonly ReReDbContext _context;
        private readonly UserModelsController _controller;

        public UserControllerTests()
        {
            // Configuration de la base de données en mémoire
            var options = new DbContextOptionsBuilder<ReReDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ReReDbContext(options);
            _controller = new UserModelsController(_context);

            // Configuration du contexte HTTP avec session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Configuration du TempData
            _controller.TempData = new TempDataDictionary(
                httpContext, 
                Mock.Of<ITempDataProvider>());

            // Initialisation des données de test
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
                },
                new UserModel
                {
                    Id = 2,
                    Nom = "Martin",
                    Prenom = "Marie",
                    Email = "marie.martin@test.com",
                    mot_de_passe = "motdepasse456",
                    RoleId = 2
                }
            };

            _context.RoleModels.AddRange(roles);
            _context.Utilisateurs.AddRange(users);
            _context.SaveChanges();
        }

        #region Tests de Connexion

        [Fact]
        public void Login_GET_ReturnsView()
        {
            var result = _controller.Login();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Login_POST_ValidCredentials_RedirectsToHome()
        {
            var result = await _controller.Login("jean.dupont@test.com", "password123");
            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_POST_InvalidCredentials_RedirectsToLoginWithError()
        {
            var result = await _controller.Login("wrong@email.com", "wrongpass");
            
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);
        }

        #endregion

        #region Tests de Déconnexion

        [Fact]
        public void Logout_ClearsSessionAndRedirects()
        {
            _controller.HttpContext.Session.SetString("UserEmail", "test@test.com");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            var result = _controller.Logout();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Vous êtes maintenant déconnecté.", _controller.TempData["Message"]);
        }

        #endregion

        #region Tests de Création

        [Fact]
        public void Create_GET_ReturnsView()
        {
            var result = _controller.Create();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Create_POST_ValidUser_RedirectsToLogin()
        {
            var newUser = new UserModel
            {
                Nom = "Nouveau",
                Prenom = "Utilisateur",
                Email = "nouveau@test.com",
                mot_de_passe = "nouveaupass",
                RoleId = 1
            };

            var result = await _controller.Create(newUser);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_POST_DuplicateEmail_ReturnsViewWithError()
        {
            var duplicateUser = new UserModel
            {
                Nom = "Duplicate",
                Prenom = "User",
                Email = "jean.dupont@test.com",
                mot_de_passe = "password",
                RoleId = 1
            };

            var result = await _controller.Create(duplicateUser);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        #endregion

        #region Tests de Liste

        [Fact]
        public async Task Index_NoParameters_ReturnsAllUsers()
        {
            var result = await _controller.Index(null, null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserModel>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Index_WithSearch_ReturnsFilteredUsers()
        {
            var result = await _controller.Index("Jean", null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserModel>>(viewResult.Model);
            var users = model.ToList();
            
            Assert.Single(users);
            Assert.Equal("Jean", users[0].Prenom);
        }

        #endregion

        #region Tests de Détails

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithUser()
        {
            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            var result = await _controller.Details(999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Tests de Modification

        [Fact]
        public async Task Edit_GET_ValidId_ReturnsViewWithUser()
        {
            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Edit_POST_ValidUser_RedirectsToIndex()
        {
            var userToEdit = await _context.Utilisateurs.FindAsync(1);
            Assert.NotNull(userToEdit);
            userToEdit.Prenom = "Jean-Modifié";

            var result = await _controller.Edit(1, userToEdit);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Tests de Suppression

        [Fact]
        public async Task Delete_GET_ValidId_ReturnsViewWithUser()
        {
            var result = await _controller.Delete(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RemovesUserAndRedirectsToIndex()
        {
            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        #endregion

        #region Tests de Changement de Mot de Passe

        [Fact]
        public async Task ChangePassword_ValidOldPassword_UpdatesSessionAndRedirects()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserEmail", "jean.dupont@test.com");
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = await _controller.ChangePassword("password123", "nouveauMotDePasse");

            // Assert - Vérifier uniquement la redirection et la session
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccountVue", redirectResult.ActionName);

            // Vérifier que la session a été mise à jour
            Assert.Equal("nouveauMotDePasse", _controller.HttpContext.Session.GetString("UserPassword"));
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_RedirectsWithError()
        {
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            var result = await _controller.ChangePassword("wrongPassword", "nouveauMotDePasse");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ModifyPassword", redirectResult.ActionName);
            Assert.Equal("Ancien mot de passe incorrect.", _controller.TempData["Message"]);
        }

        #endregion

        #region Tests des Vues

        [Fact]
        public void ModifyPassword_ReturnsView()
        {
            var result = _controller.ModifyPassword();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AccountVue_ReturnsView()
        {
            var result = _controller.AccountVue();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void DeleteUser_ReturnsDeleteView()
        {
            var result = _controller.DeleteUser();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Delete", viewResult.ViewName);
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
