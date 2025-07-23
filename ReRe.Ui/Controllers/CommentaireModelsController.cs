using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReRe.Data.DbContext;
using ReRe.Data.Models;

namespace ReRe.Ui.Controllers
{
    public class CommentaireModelsController : BaseController
    {
        private readonly ReReDbContext _context;

        public CommentaireModelsController(ReReDbContext context)
        {
            _context = context;
        }

        // GET: CommentaireModels
        public async Task<IActionResult> Index(string search, string sort)
        {
            var userId = HttpContext.Session.GetInt32("Id");
            var commentaires = _context.Commentaires
                .Include(c => c.UserModel)
                .Include(c => c.RessourceModel)
                .Include(c => c.CommentaireParent)
                .Where(c => c.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                commentaires = commentaires.Where(c =>
                    c.Comment.Contains(search) ||
                    c.UserModel.Nom.Contains(search) ||
                    c.UserModel.Prenom.Contains(search) ||
                    c.RessourceModel.Categorie.Contains(search)
                );
            }

            switch (sort)
            {
                case "auteur":
                    commentaires = commentaires.OrderBy(c => c.UserModel.Nom);
                    break;
                case "ressource":
                    commentaires = commentaires.OrderBy(c => c.RessourceModel.Categorie);
                    break;
                default:
                    commentaires = commentaires.OrderByDescending(c => c.Date);
                    break;
            }

            ViewBag.SearchQuery = search;
            ViewBag.SortOrder = sort;

            return View(await commentaires.ToListAsync());
        }


        // GET: CommentaireModels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentaireModel = await _context.Commentaires
                .Include(c => c.CommentaireParent)
                .Include(c => c.RessourceModel)
                .Include(c => c.UserModel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commentaireModel == null)
            {
                return NotFound();
            }

            return View(commentaireModel);
        }

        // GET: CommentaireModels/Create
        public IActionResult Create(int ressourceId, int? commentaireId = null)
        {
            ViewData["RessourceId"] = ressourceId;
            ViewData["CommentaireId"] = commentaireId;
            ViewData["UserId"] = new SelectList(_context.Utilisateurs, "Id", "Nom");
            return View();
        }


        // POST: CommentaireModels/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Comment,RessourceId,CommentaireId,Date")] CommentaireModel commentaireModel)
        {
            if (ModelState.IsValid)
            {
                commentaireModel.Date = DateTime.Now;
                _context.Add(commentaireModel);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "RessourceModels", new { id = commentaireModel.RessourceId });
            }
            ViewData["CommentaireId"] = new SelectList(_context.Commentaires, "Id", "Id", commentaireModel.CommentaireId);
            ViewData["RessourceId"] = new SelectList(_context.Ressources, "Id", "Categorie", commentaireModel.RessourceId);
            ViewData["UserId"] = new SelectList(_context.Utilisateurs, "Id", "Nom", commentaireModel.UserId);
            return View(commentaireModel);
        }

        // GET: CommentaireModels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentaireModel = await _context.Commentaires.FindAsync(id);
            if (commentaireModel == null)
            {
                return NotFound();
            }
            ViewData["CommentaireId"] = new SelectList(_context.Commentaires, "Id", "Id", commentaireModel.CommentaireId);
            ViewData["RessourceId"] = new SelectList(_context.Ressources, "Id", "Categorie", commentaireModel.RessourceId);
            ViewData["UserId"] = new SelectList(_context.Utilisateurs, "Id", "Nom", commentaireModel.UserId);
            return View(commentaireModel);
        }

        // POST: CommentaireModels/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Comment,RessourceId,CommentaireId,Date")] CommentaireModel commentaireModel)
        {
            if (id != commentaireModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(commentaireModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentaireModelExists(commentaireModel.Id))
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
            ViewData["CommentaireId"] = new SelectList(_context.Commentaires, "Id", "Id", commentaireModel.CommentaireId);
            ViewData["RessourceId"] = new SelectList(_context.Ressources, "Id", "Categorie", commentaireModel.RessourceId);
            ViewData["UserId"] = new SelectList(_context.Utilisateurs, "Id", "Nom", commentaireModel.UserId);
            return View(commentaireModel);
        }

        // GET: CommentaireModels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentaireModel = await _context.Commentaires
                .Include(c => c.CommentaireParent)
                .Include(c => c.RessourceModel)
                .Include(c => c.UserModel)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commentaireModel == null)
            {
                return NotFound();
            }

            return View(commentaireModel);
        }

        // POST: CommentaireModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commentaireModel = await _context.Commentaires.FindAsync(id);
            if (commentaireModel != null)
            {
                _context.Commentaires.Remove(commentaireModel);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentaireModelExists(int id)
        {
            return _context.Commentaires.Any(e => e.Id == id);
        }
    }
}
