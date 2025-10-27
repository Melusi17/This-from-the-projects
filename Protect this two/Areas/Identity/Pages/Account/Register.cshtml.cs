// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using IbhayiPharmacy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using IbhayiPharmacy.Utility;
using IbhayiPharmacy.Data;

namespace IbhayiPharmacy.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _db;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            ApplicationDbContext db,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> Rolelist { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Surname { get; set; }

            [Required]
            public string IDNumber { get; set; }

            public string? CellphoneNumber { get; set; }

            // NEW: Health Council Registration Number for Pharmacist/Pharmacy Manager
            public string? HealthCouncilRegNo { get; set; }

            public List<int> SelectedAllergyIds { get; set; } = new List<int>();

            [ValidateNever]
            public IEnumerable<SelectListItem> AllergyList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            // FIXED: Use proper async/await instead of .GetAwaiter().GetResult()
            if (!await _roleManager.RoleExistsAsync(SD.Role_Customer))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Pharmacist));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_PharmacyManager));
            }

            Input = new InputModel
            {
                Rolelist = _roleManager.Roles.Select(r => r.Name).Select(n => new SelectListItem
                {
                    Text = n,
                    Value = n
                }),
                AllergyList = _db.Active_Ingredients.Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Active_IngredientID.ToString()
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.Name = Input.Name;
                user.Surname = Input.Surname;
                user.IDNumber = Input.IDNumber;
                user.CellphoneNumber = Input.CellphoneNumber;
                user.Email = Input.Email;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("ApplicationUser created a new account with password.");

                    // Handle role assignment
                    if (!String.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }

                    // Handle customer allergies if user is a customer
                    if (Input.Role == SD.Role_Customer && Input.SelectedAllergyIds != null && Input.SelectedAllergyIds.Any())
                    {
                        // Create customer record first
                        var customer = new Customer
                        {
                            ApplicationUserId = user.Id
                        };
                        _db.Customers.Add(customer);
                        await _db.SaveChangesAsync(); // Save to get the CustomerID

                        // Add all selected allergies
                        foreach (var allergyId in Input.SelectedAllergyIds)
                        {
                            var customerAllergy = new Custormer_Allergy
                            {
                                CustomerID = customer.CustormerID,
                                Active_IngredientID = allergyId
                            };
                            _db.Custormer_Allergies.Add(customerAllergy);
                        }
                        await _db.SaveChangesAsync();
                    }

                    // NEW: Handle Pharmacist registration
                    if (Input.Role == SD.Role_Pharmacist)
                    {
                        if (string.IsNullOrEmpty(Input.HealthCouncilRegNo))
                        {
                            ModelState.AddModelError(string.Empty, "Health Council Registration Number is required for Pharmacist role.");
                            await _userManager.DeleteAsync(user); // Rollback user creation
                            return await RebuildPageAsync();
                        }

                        var pharmacist = new Pharmacist
                        {
                            ApplicationUserId = user.Id,
                            HealthCouncilRegNo = Input.HealthCouncilRegNo
                        };
                        _db.Pharmacists.Add(pharmacist);
                        await _db.SaveChangesAsync();
                    }

                    // NEW: Handle Pharmacy Manager registration
                    if (Input.Role == SD.Role_PharmacyManager)
                    {
                        if (string.IsNullOrEmpty(Input.HealthCouncilRegNo))
                        {
                            ModelState.AddModelError(string.Empty, "Health Council Registration Number is required for Pharmacy Manager role.");
                            await _userManager.DeleteAsync(user); // Rollback user creation
                            return await RebuildPageAsync();
                        }

                        var pharmacyManager = new PharmacyManager
                        {
                            ApplicationUserId = user.Id,
                            HealthCouncilRegNo = Input.HealthCouncilRegNo
                        };
                        _db.PharmacyManagers.Add(pharmacyManager);
                        await _db.SaveChangesAsync();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return await RebuildPageAsync();
        }

        private async Task<IActionResult> RebuildPageAsync()
        {
            // Rebuild the dropdown lists when returning to page
            Input.AllergyList = _db.Active_Ingredients.Select(a => new SelectListItem
            {
                Text = a.Name,
                Value = a.Active_IngredientID.ToString()
            });

            Input.Rolelist = _roleManager.Roles.Select(r => r.Name).Select(n => new SelectListItem
            {
                Text = n,
                Value = n
            });

            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}