using GestForma.Models;
using GestForma.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace GestForma.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TrainersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize(Roles = "administrateur")]
        public IActionResult AddTrainer()
        {
            return View();
        }
  

        [Authorize(Roles = "administrateur")]
        public IActionResult Trainers()
        {
            return View();
        }

        [Authorize(Roles = "administrateur")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTrainer(TrainerRegister model)
        {
            if (ModelState.IsValid)
            {
                if (model.ProfileImage == null || model.ProfileImage.Length == 0)
                {
                    // Add an error message to ModelState
                    ModelState.AddModelError("ProfileImage", "Please upload a profile image.");
                    return View("AddTrainer", model); // Return to the form with the error message
                }
                // Create the ApplicationUser object
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.Phone
                };

                // Create the user with the provided password
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "professeur");
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfileImage.CopyToAsync(memoryStream);
                        var trainer = new Trainer
                        {
                            Id_user = user.Id,
                            Field = model.Field,
                            FileName = model.ProfileImage.FileName,
                            ContentType = model.ProfileImage.ContentType,
                            Data = memoryStream.ToArray()
                        };

                        // Sauvegarder Trainer dans la base de données (via DbContext)
                        _context.Trainers.Add(trainer);
                        await _context.SaveChangesAsync();

                    }

                    // Optionally, save any other data to the database if needed

                    // Redirect or return a success message
                    TempData["Success"] = $"The user {model.Email} has been successfully added.";
                    return RedirectToAction("AddTrainer"); // Or any other page
                }

                // If creation failed, show errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            // If we reach here, something went wrong, so return the view with validation errors
            return View("AddTrainer");
        }

        [Authorize(Roles = "administrateur")]
        public async Task<IActionResult> GetImage(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer == null)
            {
                return NotFound("Trainer not found.");
            }

            return File(trainer.Data, trainer.ContentType);  // Return the image file with the content type
        }

        [Authorize(Roles = "administrateur")]
        public async Task<IActionResult> DeleteFormTrainer()
        {
            var users = _userManager.Users.ToList();
            List<List<Object>> trainers = new List<List<Object>>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "professeur") && user.archivee == false)
                {
                    var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Id_user == user.Id);

                    // Gérer les cas où le formateur n'existe pas
                    string field = trainer != null ? trainer.Field : "Not Assigned";
                    string imageUrl = trainer != null ? Url.Action("GetImage", "Trainers", new { id = trainer.Id }) : null;

                    trainers.Add(new List<Object>
            {
                user.Id,
                user.LastName,
                user.FirstName,
                user.Email,
                user.PhoneNumber,
                field,
                imageUrl
            });
                }
            }

            return View(trainers);
        }

        [Authorize(Roles = "administrateur")]
        [HttpPost]
        public async Task<IActionResult> DeleteTrainer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid ID.";
                return RedirectToAction(nameof(DeleteFormTrainer));
            }

            // Récupérer l'utilisateur
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(DeleteFormTrainer));
            }

            // Utilisation d'une transaction pour garantir l'intégrité des données
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Archiver les formations associées
                var formations = _context.Formations.Where(f => f.ID_User == user.Id).ToList();
                foreach (var formation in formations)
                {
                    formation.archivee = true;
                }

                // Archiver le formateur associé
                var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.Id_user == user.Id);
                if (trainer != null)
                {
                    trainer.archivee = true;
                }

                // Archiver l'utilisateur
                user.archivee = true;

                // Sauvegarder les modifications
                await _context.SaveChangesAsync();

                // Confirmer la transaction
                await transaction.CommitAsync();

                TempData["Success"] = $"The user {user.UserName} has been successfully deleted.";
            }
            catch (Exception ex)
            {
                // Annuler la transaction en cas d'erreur
                await transaction.RollbackAsync();
                TempData["Error"] = $"An error occurred while deleting the trainer: {ex.Message}";
            }

            return RedirectToAction(nameof(DeleteFormTrainer));
        }

        [Authorize(Roles = "professeur")]
        public async Task<IActionResult> Statistics()
        {
            var userId = _userManager.GetUserId(User); // Get the current user's ID from ClaimsPrincipal

            // Get the total number of inscriptions for the current user
            var nbrInscriTotal = await _context.Inscriptions
                .Where(ins => ins.Formation.ID_User == userId)
                .CountAsync();
            var nbrInscriTotalCerti = await _context.Inscriptions
                .Where(ins => ins.Formation.ID_User == userId && ins.Certificat)
                .CountAsync();

            // Get the inscriptions grouped by formation
            var inscriptionsByFormation = await _context.Inscriptions
                .Where(ins => ins.Formation.ID_User == userId && ins.Formation.archivee == false) // Filter by trainer's user ID
                .GroupBy(ins => new { ins.Formation.ID_Formation, ins.Formation.Intitule }) // Group by formation ID and name
                .Select(group => new FormaInscriVM
                {
                    FormationName = group.Key.Intitule,
                    Count = group.Count() // Count the inscriptions per formation
                })
                .ToListAsync();

            var inscriptions = await _context.Inscriptions
                .Where(ins => ins.User.Age != null && ins.Formation.ID_User == userId)
                .Include(ins => ins.User)  // Ensure you include the User in the query
                .ToListAsync();  // Fetch all the data first

            var ageGroups = inscriptions
                .GroupBy(ins => GetAgeGroup(ins.User.Age))  // Now we can use GetAgeGroup on the client side
                .Select(group => new
                {
                    AgeGroup = group.Key,
                    Count = group.Count()
                })
                .ToList();

            var formations = await _context.Formations
                .Where(formation => formation.ID_User == userId && formation.archivee == false)
                .ToListAsync();
            var formationsStatistics = new List<FormationsStatisticList>();
            foreach (var formation in formations)
            {
                // Get the total number of inscriptions for this formation
                var nbrInscriTotalforma = await _context.Inscriptions
                    .Where(ins => ins.Formation.ID_Formation == formation.ID_Formation)
                    .CountAsync();

                // Get the number of inscriptions with certificates for this formation
                var nbrInscriTotalCertiforma = await _context.Inscriptions
                    .Where(ins => ins.Formation.ID_Formation == formation.ID_Formation && ins.Certificat)
                    .CountAsync();

                // Get inscriptions by age group for this formation
                var inscriptionsforma = await _context.Inscriptions
                    .Where(ins => ins.Formation.ID_Formation == formation.ID_Formation && ins.User.Age != null)
                    .Include(ins => ins.User) // Ensure the User is included
                    .ToListAsync();

                var ageGroupsforma = inscriptionsforma
                     .Where(ins => ins.Formation.ID_Formation == formation.ID_Formation)
                    .GroupBy(ins => GetAgeGroup(ins.User.Age))
                    .Select(group => new AgeGroupVM
                    {
                        AgeGroup = group.Key,
                        Count = group.Count()
                    })
                    .ToList();
                formationsStatistics.Add(new FormationsStatisticList
                {
                    formationName = formation.Intitule,
                    nbrInscriTotal = nbrInscriTotalforma,
                    nbrInscriTotalCerti = nbrInscriTotalCertiforma,
                    ageGroups = ageGroupsforma
                });
            }


            // Pass the data to ViewBag
            ViewBag.nbrInscriTotal = nbrInscriTotal;
            ViewBag.nbrInscriTotalCerti = nbrInscriTotalCerti;
            ViewBag.inscriptionsByFormation = inscriptionsByFormation;
            ViewBag.ageGroups = ageGroups;
            ViewBag.formationsStatistics = formationsStatistics;

            return View();
        }
        private string GetAgeGroup(int age)
        {

            if (age < 18) return "Under 18";
            if (age <= 25) return "18-25";
            if (age <= 35) return "26-35";
            if (age <= 45) return "36-45";
            if (age <= 60) return "46-60";
            return "60+";
        }





    }





}

