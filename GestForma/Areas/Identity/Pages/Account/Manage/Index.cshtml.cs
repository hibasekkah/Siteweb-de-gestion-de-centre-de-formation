// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GestForma.Models;
using GestForma.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NuGet.DependencyResolver;

namespace GestForma.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;


        public IndexModel(
            ApplicationDbContext context,

            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 
        public string CurrentImageUrl { get; set; }
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Display(Name = "Age")]
            public int Age { get; set; }

            [Display(Name = "Field")]
            public string Field { get; set; }

            [Display(Name = "Image")]
            public IFormFile Image { get; set; }



        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstName = user.FirstName;
            var lastName = user.LastName;
            Username = userName;

            if (!User.IsInRole("professeur"))
            {
                var age = user.Age;
                Input = new InputModel
                {
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Age = age
                };
            }
            if (User.IsInRole("professeur"))
            {
                var field = _context.Trainers.FirstOrDefault(x => x.Id_user == user.Id).Field;

                var trainer = _context.Trainers.FirstOrDefault(x => x.Id_user == user.Id);

                if (trainer?.Data != null)
                {
                    var base64 = Convert.ToBase64String(trainer.Data);
                    CurrentImageUrl = $"data:{trainer.ContentType};base64,{base64}";
                }
                else
                {
                    CurrentImageUrl = null;
                }

                var age = user.Age;
                Input = new InputModel
                {
                    PhoneNumber = phoneNumber,
                    FirstName = firstName,
                    LastName = lastName,
                    Field = field
                };
            }

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            var firstName = user.FirstName;

            if (Input.FirstName != firstName)
            {
                user.FirstName = Input.FirstName;
            }

            var lastName = user.LastName;

            if (Input.LastName != lastName)
            {
                user.LastName = Input.LastName;
            }


            if (User.IsInRole("professeur"))
            {
                var field = _context.Trainers.FirstOrDefault(x => x.Id_user == user.Id).Field;

                if (Input.Field != field)
                {
                    _context.Trainers.FirstOrDefault(x => x.Id_user == user.Id).Field = Input.Field;
                }

                var trainer = _context.Trainers.FirstOrDefault(x => x.Id_user == user.Id);

                if (Input.Image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Input.Image.CopyToAsync(memoryStream);

                        trainer.FileName = Input.Image.FileName;
                        trainer.ContentType = Input.Image.ContentType;
                        trainer.Size = Input.Image.Length;
                        trainer.Data = memoryStream.ToArray();
                    }
                }
                _context.Update(trainer);
                _context.SaveChanges();
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update your profile.";
                    return RedirectToPage();
                }
                StatusMessage = "Your profile has been updated";
                return RedirectToPage();
            }
            else
            {

                var age = user.Age;

                if (Input.Age != age)
                {
                    user.Age = Input.Age;
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to update your profile.";
                    return RedirectToPage();
                }
                StatusMessage = "Your profile has been updated";
                return RedirectToPage();

            }
        }
    }
}
