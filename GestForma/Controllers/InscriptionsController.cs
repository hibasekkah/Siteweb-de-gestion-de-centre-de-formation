using Azure;
using GestForma.Models;
using GestForma.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace GestForma.Controllers
{
    public class InscriptionsController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        // Injection des services UserManager et RoleManager
        public InscriptionsController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public async Task<IActionResult> CourseRegistration(int CourseId, string ParticipantId)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(ParticipantId) || CourseId <= 0)
            {
                // Return an error view or redirect to a page with an error message
                return BadRequest("Invalid CourseId or ParticipantId.");
            }

            var result = await _context.Inscriptions.FirstOrDefaultAsync(element => element.User.Id == ParticipantId && element.ID_Formation == CourseId && !element.Certificat);
            if (result != null) {
                TempData["Error"] = "You are already registered for this course and have not yet received a certificate. After receiving the certificate, you can register for this course again if you wish.";
                return RedirectToAction("Index", "Home");
            }
            // Create a new inscription
            var newInscription = new Inscription
            {
                ID_User = ParticipantId,
                ID_Formation = CourseId
            };

            // Add to the database and save changes
            _context.Add(newInscription);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Registration confirmed. Please visit the center to complete the payment for this course.";
            // Redirect to the index page or a confirmation view
            return RedirectToAction("Index","Home");
        }
        public async Task<IActionResult> CourseRegister(int CourseId, string ParticipantId)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(ParticipantId) || CourseId <= 0)
            {
                // Return an error view or redirect to a page with an error message
                return BadRequest("Invalid CourseId or ParticipantId.");
            }

            var result = await _context.Inscriptions.FirstOrDefaultAsync(element => element.User.Id == ParticipantId && element.ID_Formation == CourseId && !element.Certificat);
            if (result != null)
            {
                TempData["Error"] = "You are already registered for this course and have not yet received a certificate. After receiving the certificate, you can register for this course again if you wish.";
                return RedirectToAction("Courses", "Courses");
            }
            // Create a new inscription
            var newInscription = new Inscription
            {
                ID_User = ParticipantId,
                ID_Formation = CourseId
            };

            // Add to the database and save changes
            _context.Add(newInscription);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Registration confirmed. Please visit the center to complete the payment for this course.";
            // Redirect to the index page or a confirmation view
            return RedirectToAction("Courses", "Courses");
        }

        public async Task<IActionResult> CoursePaid(int id)
        {
            // Fetch the inscription asynchronously and ensure it's found
            var inscription = await _context.Inscriptions.FindAsync(id);

            if (inscription == null)
            {
                // Handle the case when the inscription is not found, e.g., show an error message
                TempData["Error"] = "Inscription not found!";
                return RedirectToAction("Payement", "Home");
            }

            // Mark the course as paid (or certified)
            inscription.Paiement = true;

            // Save changes asynchronously
            _context.Update(inscription);
            await _context.SaveChangesAsync();
            TempData["Success"] = "The payment has been successfully marked as completed.";
            // Redirect to the Payement action after the update
            return RedirectToAction("Payement", "Home");
        }
      


        public async Task<IActionResult> Liste()
        {
            var inscriptionsFini = await _context.Inscriptions
                                          .Include(i => i.Formation)
                                          .Where(i => i.ID_User == _userManager.GetUserId(User) && i.Fin == true && i.Certificat == true && i.Paiement == true && i.archivee==false)
                                         .ToListAsync();

            var inscriptionsnonFini = await _context.Inscriptions
                                              .Include(i => i.Formation)
                                              .Where(i => i.ID_User == _userManager.GetUserId(User) && i.Fin == false && i.archivee == false && i.Paiement==true)
                                             .ToListAsync();

            var inscriptionsNonPayes = await _context.Inscriptions
                                              .Include(i => i.Formation)
                                              .Where(i => i.ID_User == _userManager.GetUserId(User) && i.Paiement == false && i.archivee == false)
                                             .ToListAsync();

            //i.ID_User == _userManager.GetUserId(User) &&

            ViewData["inscriptionsFini"] = inscriptionsFini;
            ViewData["inscriptionsnonFini"] = inscriptionsnonFini;
            ViewData["inscriptionsNonPayes"] = inscriptionsNonPayes;

            var userRatings = await _context.Rates
                                    .Where(r => r.ID_User == _userManager.GetUserId(User) && r.archivee == false)
                                    .ToDictionaryAsync(r => r.ID_Formation, r => (int)r.ContenuRate);

            ViewData["inscriptionsFini"] = inscriptionsFini;
            ViewData["Ratings"] = userRatings;

            return View();
        }



        public async Task<IActionResult> SaveRating(int formationId, float rateValue)
        {
            // Obtenir le userId de l'utilisateur connecté
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Vérifier si l'utilisateur est authentifié
            if (string.IsNullOrEmpty(userId))
            {
                // Gérer le cas où l'utilisateur n'est pas authentifié
                return BadRequest("User is not authenticated.");
            }

            // Vérifier si l'utilisateur existe dans la table AspNetUsers
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                // Gérer le cas où l'utilisateur n'existe pas
                return BadRequest("User does not exist.");
            }

            // Vérifier si l'évaluation existe déjà pour cette formation et cet utilisateur
            var existingRating = await _context.Rates
            .FirstOrDefaultAsync(r => r.ID_User == userId && r.ID_Formation == formationId && r.archivee == false);

            if (existingRating != null)
            {
                // Mettre à jour l'évaluation existante
                existingRating.ContenuRate = rateValue;
                _context.Rates.Update(existingRating);
            }
            else
            {
                // Ajouter une nouvelle évaluation
                var rate = new Rate
                {
                    ID_User = userId,
                    ID_Formation = formationId,
                    ContenuRate = rateValue
                };
                await _context.Rates.AddAsync(rate);
            }

            // Sauvegarder dans la base de données
            await _context.SaveChangesAsync();

            // Rediriger vers la page Liste après l'enregistrement
            return RedirectToAction("Liste");
        }




}



     



    }
