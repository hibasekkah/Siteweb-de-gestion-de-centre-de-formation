using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestForma.Models;
using GestForma.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Authorization;

namespace GestForma.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CoursesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
            {
                _context = context;
                _userManager = userManager;
                _webHostEnvironment = webHostEnvironment;
            }

        // GET: Courses
        [Authorize(Roles = "professeur")]
        public async Task<IActionResult> Index()
        {
            
            var userId = _userManager.GetUserId(User);

            var applicationDbContext = _context.Formations
                 .Where(f => f.ID_User == userId && f.archivee == false)  // Ajout de la condition archivee == false
                 .Include(f => f.Categorie)
                 .Include(f => f.User)
                 .AsQueryable();


            return View(await applicationDbContext.ToListAsync());
        }



        // GET: Courses/Details/5
        [Authorize(Roles = "professeur")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formation = await _context.Formations
                .Include(f => f.Categorie)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ID_Formation == id);
            if (formation == null)
            {
                return NotFound();
            }

            return View(formation);
        }
        [Authorize(Roles = "professeur")]
        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var formation = await _context.Formations.FindAsync(id);
            if (formation == null)
            {
                return NotFound();
            }

            return File(formation.Data,formation.ContentType);  // Retourner l'image avec le type MIME
        }

        // GET: Courses/Create
        [Authorize(Roles = "professeur")]
        public IActionResult Create()
        {
            ViewData["Id_Categorie"] = new SelectList(_context.Categories.Where(c => c.archivee == false), "Id", "Title");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, [Bind("ID_Formation,Intitule,Description,Id_Categorie,Duree,Cout")] Formation formation)
        {


            if (ModelState.IsValid)
            {

                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        formation.FileName = file.FileName;
                        formation.ContentType = file.ContentType;
                        formation.Size = file.Length;
                        formation.Data = memoryStream.ToArray();
                    }
                }

                formation.ID_User = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(formation.ID_User))
                {
                    return Unauthorized();
                }
                
                _context.Add(formation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course successfully added!";
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Validation error: {error.ErrorMessage}");
                ModelState.AddModelError("", error.ErrorMessage);
            }

            ViewData["Id_Categorie"] = new SelectList(_context.Categories.Where(c => c.archivee == false), "Id", "Title", formation.Id_Categorie);

            return View(formation);
        }


        [Authorize(Roles = "professeur")]
        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formation = await _context.Formations.FindAsync(id);
            if (formation == null)
            {
                return NotFound();
            }
            ViewData["Id_Categorie"] = new SelectList(_context.Categories, "Id", "Title", formation.Id_Categorie);
            return View(formation);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Courses/Edit/5
        [Authorize(Roles = "professeur")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile file, [Bind("ID_Formation,Intitule,Description,Id_Categorie,Duree,Cout,FileName,ContentType,Size,Data")] Formation formation)
        {
            if (id != formation.ID_Formation)
            {
                return NotFound();
            }

            ModelState.Remove("file"); // Supprime la validation du fichier

            if (ModelState.IsValid)
            {
                try
                {
                    // Récupérer la formation existante en mode tracking
                    var existingFormation = await _context.Formations
                        .FirstOrDefaultAsync(f => f.ID_Formation == id);

                    if (existingFormation == null)
                    {
                        return NotFound();
                    }

                    // Mettre à jour les propriétés de base
                    existingFormation.Intitule = formation.Intitule;
                    existingFormation.Description = formation.Description;
                    existingFormation.Id_Categorie = formation.Id_Categorie;
                    existingFormation.Duree = formation.Duree;
                    existingFormation.Cout = formation.Cout;

                    // Gérer le fichier
                    if (file != null)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            existingFormation.FileName = file.FileName;
                            existingFormation.ContentType = file.ContentType;
                            existingFormation.Size = file.Length;
                            existingFormation.Data = memoryStream.ToArray();
                        }
                    }
                    // Si pas de nouveau fichier, on garde l'ancien
                    // Pas besoin de code ici car on ne modifie pas les propriétés du fichier

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Course successfully updated!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FormationExists(formation.ID_Formation))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // En cas d'erreur
            ViewData["Id_Categorie"] = new SelectList(_context.Categories, "Id", "Title", formation.Id_Categorie);
            return View(formation);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "professeur")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var formation = await _context.Formations
                .Include(f => f.Categorie)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.ID_Formation == id);
            if (formation == null)
            {
                return NotFound();
            }

            return View(formation);
        }

        // POST: Courses/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var formation = await _context.Formations.FindAsync(id);

            if (formation != null)
            {
                // Archiver la formation
                formation.archivee = true;

                // Rechercher les inscriptions associées à l'utilisateur et les archiver
                var inscriptions = await _context.Inscriptions
                    .Where(inscription => inscription.ID_Formation == id) // Remplacez `IdUser` par le nom exact de la propriété dans votre modèle
                    .ToListAsync();

                foreach (var inscription in inscriptions)
                {
                    inscription.archivee = true;
                }

                // Message de succès
                TempData["Success"] = "Course and associated enrollments successfully archived!";
            }
            else
            {
                TempData["Error"] = "Course not found!";
            }

            // Sauvegarder les modifications dans la base de données
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FormationExists(int id)
        {
            return _context.Formations.Any(e => e.ID_Formation == id);
        }



        public async Task<IActionResult> Courses(string searchBy, string keyword)
        {
            // Calculer la moyenne des évaluations des formations
            var averageRates = await _context.Rates
                .GroupBy(r => r.ID_Formation)
                .Select(g => new
                {
                    ID_Formation = g.Key,
                    AverageRate = g.Average(r => r.ContenuRate),
                    TotalVotes = g.Count()
                })
                .ToDictionaryAsync(x => x.ID_Formation, x => new { x.AverageRate, x.TotalVotes });


            var formations = await _context.Formations
                .Where(f => f.archivee == false)
                .Include(f => f.Categorie)
                .Include(f => f.User)
                .ToListAsync();

            if (!string.IsNullOrEmpty(keyword))
            {
                switch (searchBy?.ToLower())
                {
                    case "categorie":
                        formations = formations
                            .Where(f => f.Categorie.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) && f.archivee==false)
                            .ToList();
                        break;
                    case "prix":
                        if (float.TryParse(keyword, out float prix))
                        {
                            formations = formations
                                .Where(f => f.Cout <= prix && f.archivee == false)
                                .ToList();
                        }
                        break;
                    case "titre":
                        formations = formations
                            .Where(f => f.Intitule.Contains(keyword, StringComparison.OrdinalIgnoreCase) && f.archivee == false)
                            .ToList();
                        break;
                    case "nombreheure":
                        if (int.TryParse(keyword, out int nombreHeure))
                        {
                            formations = formations
                                .Where(f => f.Duree == nombreHeure && f.archivee == false)
                                .ToList();
                        }
                        break;
                }
            }


            var result = formations.Where( f =>f.archivee == false).Select(f => new
            {
                ID_Formation = f.ID_Formation,
                Intitule = f.Intitule ?? "No Title Available",
                Duree = f.Duree,
                Cout = f.Cout,
                CategorieTitle = f.Categorie?.Title ?? "Uncategorized",
                FormateurName = f.User?.FirstName ?? "No Instructor Assigned",
                AverageRate = averageRates.ContainsKey(f.ID_Formation) ? averageRates[f.ID_Formation].AverageRate : 0,

                Data = f.Data,
                ContentType = f.ContentType
            }).ToList();


            return View(result);
        }






    }
}

