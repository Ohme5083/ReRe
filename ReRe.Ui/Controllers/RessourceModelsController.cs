using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReRe.Data.DbContext;
using ReRe.Data.Models;
using System.Linq;

namespace ReRe.Ui.Controllers
{
    public class RessourceModelsController : BaseController
    {
        private readonly ReReDbContext _context;

        public RessourceModelsController(ReReDbContext context)
        {
            _context = context;
        }

        // GET: RessourceModels
        public async Task<IActionResult> Index(string search, string sort, RessourceModel r)
        {
            // Récupération des infos utilisateur
            ViewBag.Email = HttpContext.Session.GetString("UserEmail");
            ViewBag.Nom = HttpContext.Session.GetString("UserName");
            ViewBag.Prenom = HttpContext.Session.GetString("UserFirstName");
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            ViewBag.RoleId = HttpContext.Session.GetInt32("RoleId");

            // Préparation de la requête
            var ressources = _context.Ressources.AsQueryable();

            // Filtrage
            if (!string.IsNullOrWhiteSpace(search))
            {
                ressources = ressources.Where(r =>
                    r.Name.Contains(search) ||
                    r.Description.Contains(search) ||
                    r.Type.Libelle.Contains(search) ||
                    r.Categorie.Contains(search));
            }

            // Tri
            switch (sort)
            {
                case "categorie":
                    ressources = ressources.OrderBy(r => r.Categorie);
                    break;
                case "type":
                    ressources = ressources.OrderBy(r => r.Type);
                    break;
                case "date":
                    ressources = ressources.OrderByDescending(r => r.CreationDate);
                    break;
                case "nom":
                default:
                    ressources = ressources.OrderBy(r => r.Name);
                    break;
            }

            ViewBag.SearchQuery = search;
            ViewBag.SortOrder = sort;

            return View(await ressources.ToListAsync());
        }


        // GET: RessourceModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var ressourceModel = await _context.Ressources
                .Include(r => r.Commentaires)
                .ThenInclude(c => c.UserModel)
                .Include(r => r.Utilisateurs)   // <-- Ajout ici
                .FirstOrDefaultAsync(m => m.Id == id);


            if (ressourceModel == null)
                return NotFound();

            // Charger tous les commentaires de la ressource
            var commentaires = await _context.Commentaires
                .Where(c => c.RessourceId == id)
                .Include(c => c.UserModel)
                .ToListAsync();

            // Construire la hiérarchie en mémoire
            foreach (var commentaire in commentaires)
            {
                commentaire.CommentairesEnfants = commentaires
                    .Where(c => c.CommentaireId == commentaire.Id)
                    .ToList();
            }

            // N’afficher que les racines
            ressourceModel.Commentaires = commentaires
                .Where(c => c.CommentaireId == null)
                .ToList();
            // Liste des utilisateurs pour ajout
            ViewBag.AllUsers = await _context.Users.ToListAsync();

            return View(ressourceModel);
        }



        private async Task<List<SelectListItem>> GetTypeSelectListAsync()
        {
            return await _context.TypeModel
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Libelle
                })
                .ToListAsync();
        }

        // GET: RessourceModels/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Types = await GetTypeSelectListAsync();
            return View();
        }

        // POST: RessourceModels/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Categorie,TypeId")] RessourceModel ressourceModel)
        {
            if (ModelState.IsValid)
            {
                ressourceModel.Creator = ViewBag.Nom;
                ressourceModel.CreationDate = DateTime.Now;
                _context.Add(ressourceModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Types = await GetTypeSelectListAsync();
            return View(ressourceModel);
        }

        // GET: RessourceModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ressourceModel = await _context.Ressources.FindAsync(id);
            if (ressourceModel == null) return NotFound();

            ViewBag.Types = await GetTypeSelectListAsync();
            return View(ressourceModel);
        }

        // POST: RessourceModels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Categorie,TypeId,CreationDate")] RessourceModel ressourceModel)
        {
            if (id != ressourceModel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ressourceModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RessourceModelExists(ressourceModel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Types = await GetTypeSelectListAsync();
            return View(ressourceModel);
        }


        // GET: RessourceModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ressourceModel = await _context.Ressources
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ressourceModel == null)
            {
                return NotFound();
            }

            return View(ressourceModel);
        }

        // POST: RessourceModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ressourceModel = await _context.Ressources.FindAsync(id);
            if (ressourceModel != null)
            {
                _context.Ressources.Remove(ressourceModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RessourceModelExists(int id)
        {
            return _context.Ressources.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> AssignUsers(int ressourceId, List<int> userIds)
        {
            var ressource = await _context.Ressources
                .Include(r => r.Utilisateurs)
                .FirstOrDefaultAsync(r => r.Id == ressourceId);

            if (ressource == null)
                return NotFound();

            var selectedUsers = await _context.Utilisateurs.Where(u => userIds.Contains(u.Id)).ToListAsync();

            ressource.Utilisateurs.Clear();
            foreach (var user in selectedUsers)
            {
                ressource.Utilisateurs.Add(user);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = ressourceId });
        }

    }
}
