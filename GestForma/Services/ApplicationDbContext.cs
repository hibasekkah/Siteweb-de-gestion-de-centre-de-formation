using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using GestForma.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GestForma.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            var administrateur = new IdentityRole("administrateur");
            administrateur.NormalizedName = "administrateur";


            var professeur = new IdentityRole("professeur");
            professeur.NormalizedName = "professeur";

            var participant = new IdentityRole("participant");
            participant.NormalizedName = "participant";

            var invité = new IdentityRole("invité");
            invité.NormalizedName = "invité";

            builder.Entity<IdentityRole>().HasData(administrateur, professeur, participant, invité);

            

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        // Ajouter un DbSet pour chaque entité
        public DbSet<Actualite> Actualites { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CommentairesEntiers> CommentairesEntiers { get; set; }
        public DbSet<Formation> Formations { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Rate> Rates { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Message> Messages { get; set; }

    }
}
