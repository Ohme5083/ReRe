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
    public class UserAccountTests : IDisposable
    {
        private readonly ReReDbContext _context;
        private readonly UserModelsController _controller;

        public UserAccountTests()
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

            // Configuration du TempData pour éviter les NullReferenceException
            _controller.TempData = new TempDataDictionary(
                httpContext, 
                Mock.Of<ITempDataProvider>());

            // Initialisation des données de test
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Créer des rôles de test
            var roles = new List<RoleModel>
            {
                new RoleModel { Id = 1, Libelle = "Utilisateur" },
                new RoleModel { Id = 2, Libelle = "Administrateur" }
            };

            // Créer des utilisateurs de test
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

        #region Tests de Connexion (Login)

        [Fact]
        public void Login_GET_ReturnsView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Vue par défaut
        }

        [Fact]
        public async Task Login_POST_ValidCredentials_RedirectsToHome()
        {
            // Arrange
            string email = "jean.dupont@test.com";
            string password = "password123";

            // Act
            var result = await _controller.Login(email, password);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Vérifier que les informations sont stockées en session
            Assert.Equal(email, _controller.HttpContext.Session.GetString("UserEmail"));
            Assert.Equal("Dupont", _controller.HttpContext.Session.GetString("UserName"));
            Assert.Equal("Jean", _controller.HttpContext.Session.GetString("UserFirstName"));
            
            // Vérifier que les IDs sont stockés (sans vérifier la valeur exacte)
            Assert.NotNull(_controller.HttpContext.Session.GetInt32("Id"));
            Assert.NotNull(_controller.HttpContext.Session.GetInt32("RoleId"));
        }

        [Fact]
        public async Task Login_POST_InvalidEmail_RedirectsToLoginWithError()
        {
            // Arrange
            string email = "inexistant@test.com";
            string password = "password123";

            // Act
            var result = await _controller.Login(email, password);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);

            // Vérifier qu'aucune session n'est créée
            Assert.Null(_controller.HttpContext.Session.GetString("UserEmail"));
        }

        [Fact]
        public async Task Login_POST_InvalidPassword_RedirectsToLoginWithError()
        {
            // Arrange
            string email = "jean.dupont@test.com";
            string password = "mauvais_mot_de_passe";

            // Act
            var result = await _controller.Login(email, password);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Erreur : email ou mot de passe incorrect.", _controller.TempData["Message"]);
        }

        #endregion

        #region Tests de Déconnexion (Logout)

        [Fact]
        public void Logout_ClearsSessionAndRedirectsToLogin()
        {
            // Arrange - Simuler une session active
            _controller.HttpContext.Session.SetString("UserEmail", "test@test.com");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Vous êtes maintenant déconnecté.", _controller.TempData["Message"]);

            // Vérifier que la session est vidée
            Assert.Null(_controller.HttpContext.Session.GetString("UserEmail"));
            Assert.Null(_controller.HttpContext.Session.GetInt32("Id"));
        }

        #endregion

        #region Tests de Création de Compte (Create)

        [Fact]
        public void Create_GET_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task Create_POST_ValidUser_RedirectsToLogin()
        {
            // Arrange
            var newUser = new UserModel
            {
                Nom = "Nouveau",
                Prenom = "Utilisateur",
                Email = "nouveau@test.com",
                mot_de_passe = "nouveaupass",
                RoleId = 1
            };

            // Act
            var result = await _controller.Create(newUser);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            // Vérifier que l'utilisateur a été créé en base
            var userInDb = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == "nouveau@test.com");
            Assert.NotNull(userInDb);
            Assert.Equal("Nouveau", userInDb.Nom);
            Assert.Equal("Utilisateur", userInDb.Prenom);
            Assert.Equal(1, userInDb.RoleId); // RoleId forcé à 1
        }

        [Fact]
        public async Task Create_POST_DuplicateEmail_ReturnsViewWithError()
        {
            // Arrange - Utiliser un email qui existe déjà
            var duplicateUser = new UserModel
            {
                Nom = "Duplicate",
                Prenom = "User",
                Email = "jean.dupont@test.com", // Email déjà existant
                mot_de_passe = "password",
                RoleId = 1
            };

            // Act
            var result = await _controller.Create(duplicateUser);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Email"));
            Assert.Equal("Cet email est déjà utilisé.", 
                _controller.ModelState["Email"]?.Errors[0].ErrorMessage);
        }

        #endregion

        #region Tests de Liste des Utilisateurs (Index)

        [Fact]
        public async Task Index_NoParameters_ReturnsAllUsers()
        {
            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserModel>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // 2 utilisateurs de test
        }

        [Fact]
        public async Task Index_WithSearch_ReturnsFilteredUsers()
        {
            // Act
            var result = await _controller.Index("Jean", null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserModel>>(viewResult.Model);
            var users = model.ToList();
            
            Assert.Single(users);
            Assert.Equal("Jean", users[0].Prenom);
            
            // Corriger l'accès au ViewBag - utiliser le contrôleur, pas le ViewResult
            Assert.Equal("Jean", _controller.ViewBag.SearchQuery);
        }

        [Fact]
        public async Task Index_WithSortByRole_ReturnsSortedUsers()
        {
            // Act
            var result = await _controller.Index(null, "role");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<UserModel>>(viewResult.Model);
            
            // Corriger l'accès au ViewBag - utiliser le contrôleur, pas le ViewResult
            Assert.Equal("role", _controller.ViewBag.SortOrder);
        }

        #endregion

        #region Tests de Détails (Details)

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithUser()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Jean", model.Prenom);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Tests de Modification (Edit)

        [Fact]
        public async Task Edit_GET_ValidId_ReturnsViewWithUser()
        {
            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Edit_POST_ValidUser_RedirectsToIndex()
        {
            // Arrange
            var userToEdit = await _context.Utilisateurs.FindAsync(1);
            Assert.NotNull(userToEdit);
            userToEdit.Prenom = "Jean-Modifié";

            // Act
            var result = await _controller.Edit(1, userToEdit);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Vérifier que la modification a été sauvegardée
            var updatedUser = await _context.Utilisateurs.FindAsync(1);
            Assert.NotNull(updatedUser);
            Assert.Equal("Jean-Modifié", updatedUser.Prenom);
        }

        #endregion

        #region Tests de Suppression (Delete)

        [Fact]
        public async Task Delete_GET_ValidId_ReturnsViewWithUser()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserModel>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RemovesUserAndRedirectsToIndex()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Vérifier que l'utilisateur a été supprimé
            var deletedUser = await _context.Utilisateurs.FindAsync(1);
            Assert.Null(deletedUser);
        }

        #endregion

        #region Tests de Changement de Mot de Passe

        [Fact]
        public async Task ChangePassword_ValidOldPassword_UpdatesPasswordAndRedirectsToAccount()
        {
            // Arrange - Simuler une session active
            _controller.HttpContext.Session.SetString("UserEmail", "jean.dupont@test.com");
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = await _controller.ChangePassword("password123", "nouveauMotDePasse");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccountVue", redirectResult.ActionName);

            // Vérifier que le mot de passe a été mis à jour
            var user = await _context.Utilisateurs.FindAsync(1);
            Assert.NotNull(user);
            Assert.Equal("nouveauMotDePasse", user.mot_de_passe);

            // Vérifier que la session a été mise à jour
            Assert.Equal("nouveauMotDePasse", _controller.HttpContext.Session.GetString("UserPassword"));
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_RedirectsToModifyPasswordWithError()
        {
            // Arrange
            _controller.HttpContext.Session.SetString("UserPassword", "password123");
            _controller.HttpContext.Session.SetInt32("Id", 1);

            // Act
            var result = await _controller.ChangePassword("mauvaisMotDePasse", "nouveauMotDePasse");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ModifyPassword", redirectResult.ActionName);
            Assert.Equal("Ancien mot de passe incorrect.", _controller.TempData["Message"]);
        }

        #endregion

        #region Tests des Vues Simples

        [Fact]
        public void ModifyPassword_ReturnsView()
        {
            // Act
            var result = _controller.ModifyPassword();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void AccountVue_ReturnsView()
        {
            // Act
            var result = _controller.AccountVue();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void DeleteUser_ReturnsDeleteView()
        {
            // Act
            var result = _controller.DeleteUser();

            // Assert
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
