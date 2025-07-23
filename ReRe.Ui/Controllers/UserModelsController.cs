using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReRe.Data.DbContext;
using ReRe.Data.Models;

namespace ReRe.Ui.Controllers
{
    public class UserModelsController : BaseController
    {
        private readonly ReReDbContext _context;

        public UserModelsController(ReReDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        // GET: UserModels
        public async Task<IActionResult> Index(string search, string sort)
        {
            // Préparer la requête
            var utilisateurs = _context.Utilisateurs
                .Include(u => u.Role)
                .AsQueryable();

            // Filtrer si une recherche est fournie
            if (!string.IsNullOrWhiteSpace(search))
            {
                utilisateurs = utilisateurs.Where(u =>
                    u.Nom.Contains(search) ||
                    u.Prenom.Contains(search) ||
                    u.Email.Contains(search));
            }

            // Tri selon le paramètre
            switch (sort)
            {
                case "role":
                    utilisateurs = utilisateurs.OrderBy(u => u.Role.Libelle);
                    break;
                case "nom":
                default:
                    utilisateurs = utilisateurs.OrderBy(u => u.Nom).ThenBy(u => u.Prenom);
                    break;
            }

            // Pour garder les valeurs dans la vue
            ViewBag.SearchQuery = search;
            ViewBag.SortOrder = sort;

            return View(await utilisateurs.ToListAsync());
        }


        public IActionResult ModifyPassword()
        {
            return View();
        }

        public IActionResult DeleteUser()
        {
            return View("Delete");
        }

        public IActionResult AccountVue()
        {
            return View();
        }

        // GET: UserModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userModel = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // GET: UserModels/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: UserModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Email,mot_de_passe,RoleId")] UserModel userModel)
        {
            userModel.RoleId = 1;  // Initialisation forcée à 1

            if (ModelState.IsValid)
            {
                // Vérifier si un utilisateur avec le même email existe déjà
                var existingUser = await _context.Utilisateurs
                    .FirstOrDefaultAsync(u => u.Email == userModel.Email);

                if (existingUser != null)
                {
                    // Ajouter une erreur au ModelState
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");

                    // Retourne la vue avec l’erreur affichée
                    return View(userModel);
                }

                _context.Add(userModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            ViewData["RoleIds"] = new SelectList(_context.RoleModels, "Id", "Libelle", userModel.RoleId);
            return View(userModel);
        }


        // GET: UserModels/mail
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            Console.WriteLine("GetByMail");
            var userModel = await _context.Utilisateurs
                            .Include(u => u.Role)
                            .FirstOrDefaultAsync(m => m.Email == email);

            if (userModel == null || userModel.mot_de_passe != password)
            {
                TempData["Message"] = "Erreur : email ou mot de passe incorrect.";
                Console.WriteLine($"Connexion echoué");
                return RedirectToAction("Login");
            }
            else
            {
                HttpContext.Session.SetString("UserEmail", userModel.Email ?? "");
                HttpContext.Session.SetString("UserPassword", userModel.mot_de_passe);
                HttpContext.Session.SetString("UserName", userModel.Nom);
                HttpContext.Session.SetString("UserFirstName", userModel.Prenom);
                HttpContext.Session.SetInt32("Id", userModel.Id);
                HttpContext.Session.SetInt32("RoleId", userModel.RoleId);

                Console.WriteLine($"Connexion réussie : Email = {userModel.Email}, RoleId = {userModel.RoleId}");

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: UserModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userModel = await _context.Utilisateurs.FindAsync(id);
            if (userModel == null)
            {
                return NotFound();
            }
            ViewData["RoleIds"] = new SelectList(_context.RoleModels, "Id", "Libelle", userModel.RoleId);
            return View(userModel);
        }

        // POST: UserModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Email,mot_de_passe,RoleId")] UserModel userModel)
        {
            if (id != userModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserModelExists(userModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleIds"] = new SelectList(_context.RoleModels, "Id", "Libelle", userModel.RoleId);
            return View(userModel);
        }


        // GET: UserModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userModel = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userModel == null)
            {
                return NotFound();
            }

            return View(userModel);
        }

        // POST: UserModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Console.WriteLine("DeleteConfirmed");
            var userModel = await _context.Utilisateurs.FindAsync(id);
            if (userModel != null)
            {
                _context.Utilisateurs.Remove(userModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserModelExists(int id)
        {
            return _context.Utilisateurs.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string ancienMotDePasse, string nouveauMotDePasse)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            var id = HttpContext.Session.GetInt32("Id");
            var motDePasseActuel = HttpContext.Session.GetString("UserPassword");

            if (motDePasseActuel != ancienMotDePasse)
            {
                TempData["Message"] = "Ancien mot de passe incorrect.";
                return RedirectToAction("ModifyPassword");
            }

            if (ModelState.IsValid && id != null)
            {
                var user = await _context.Utilisateurs.FindAsync(id.Value);
                // Remplace 'Users' par ta DbSet réelle
                if (user != null)
                {
                    user.mot_de_passe = nouveauMotDePasse; // ou user.MotDePasse selon ton modèle
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.SetString("UserPassword", nouveauMotDePasse); // Met à jour la session aussi
                    return RedirectToAction("AccountVue");
                }
            }

            TempData["Message"] = "Erreur lors du changement de mot de passe.";
            return RedirectToAction("AccountVue");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Effacer les informations de session pour déconnecter l'utilisateur
            HttpContext.Session.Clear();

            // Afficher un message de confirmation de déconnexion
            TempData["Message"] = "Vous êtes maintenant déconnecté.";

            // Rediriger vers la page de connexion
            return RedirectToAction("Login");
        }



    }
}
