using GestForma.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GestForma.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.GetUserAsync(context.Principal);
            if (user != null && user.archivee)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            }
        }
    };
});

builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();

        var user = await userManager.GetUserAsync(context.User);

        if (user != null)
        {
            // RoleProfesseur�rifiez si l'utilisateur a le r�le "invit�"
            if (await userManager.IsInRoleAsync(user, "invit�"))
            {
                await signInManager.SignOutAsync(); // D�connectez l'utilisateur
                context.Response.Redirect("/"); // Redirigez vers une page d'acc�s refus�
                return;
            }

            // RoleProfesseur�rifiez si l'utilisateur a le r�le "participant"
           
        }
    }

    // Continuez le pipeline si aucune condition n'est remplie
    await next.Invoke();
});
*/
app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = context.RequestServices.GetRequiredService<SignInManager<ApplicationUser>>();
        var tempData = context.RequestServices.GetService<ITempDataDictionaryFactory>().GetTempData(context);

        var user = await userManager.GetUserAsync(context.User);

        if (user != null && await userManager.IsInRoleAsync(user, "invité"))
        {
            // Ajoutez un message pour l'utilisateur
            tempData["ErrorMessage"] = "Votre compte est en attente de validation par un administrateur.";

            await signInManager.SignOutAsync(); // D�connectez l'utilisateur
            context.Response.Redirect(context.Request.Path); // Rechargez la page actuelle
            return;
        }
    }

    await next.Invoke();
});




app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapDefaultControllerRoute();
});

builder.Logging.AddConsole();
builder.Logging.AddDebug();

app.Run();




