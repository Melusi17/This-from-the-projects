using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.ViewModel;
using IbhayiPharmacy.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IbhayiPharmacy.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userList = await _db.ApplicationUsers.ToListAsync();

                foreach (var user in userList)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    user.Role = string.Join(", ", userRoles);

                    var userClaims = await _userManager.GetClaimsAsync(user);
                    user.UserClaim = string.Join(", ", userClaims.Select(c => c.Type));
                }

                return View(userList);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading users: {ex.Message}";
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> ManagerRole(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User ID is required";
                    return RedirectToAction(nameof(Index));
                }

                IdentityUser user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                List<string> existingUserRoles = (await _userManager.GetRolesAsync(user)).ToList();
                var model = new RolesViewModel()
                {
                    User = user
                };

                // Get all roles from database
                var allRoles = await _roleManager.Roles.ToListAsync();

                foreach (var role in allRoles)
                {
                    RoleSelection roleSelection = new()
                    {
                        RoleName = role.Name,
                        IsSelected = existingUserRoles.Contains(role.Name)
                    };
                    model.RoleList.Add(roleSelection);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading role management: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerRole(RolesViewModel rolesViewModel)
        {
            try
            {
                if (rolesViewModel?.User == null || string.IsNullOrEmpty(rolesViewModel.User.Id))
                {
                    TempData["Error"] = "Invalid user data";
                    return RedirectToAction(nameof(Index));
                }

                IdentityUser user = await _userManager.FindByIdAsync(rolesViewModel.User.Id);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                // Get current roles
                var oldRoles = await _userManager.GetRolesAsync(user);

                // Remove from old roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, oldRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Error while removing existing roles: " +
                                        string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return await ManagerRole(rolesViewModel.User.Id);
                }

                // Add to selected roles
                var selectedRoles = rolesViewModel.RoleList?
                    .Where(x => x.IsSelected)
                    .Select(y => y.RoleName)
                    .ToList() ?? new List<string>();

                if (selectedRoles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                    if (!addResult.Succeeded)
                    {
                        TempData["Error"] = "Error while adding new roles: " +
                                            string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return await ManagerRole(rolesViewModel.User.Id);
                    }
                }

                TempData["Success"] = "Roles assigned successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error managing roles: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ManagerUserClaim(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User ID is required";
                    return RedirectToAction(nameof(Index));
                }

                IdentityUser user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                var existingUserClaims = await _userManager.GetClaimsAsync(user);
                var model = new ClaimsViewModel()
                {
                    User = user
                };

                foreach (Claim claim in ClaimsStore.claimsList)
                {
                    ClaimSelection userClaim = new()
                    {
                        ClaimType = claim.Type,
                        IsSelected = existingUserClaims.Any(c => c.Type == claim.Type)
                    };
                    model.ClaimList.Add(userClaim);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading claims management: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerUserClaim(ClaimsViewModel claimsViewModel)
        {
            try
            {
                if (claimsViewModel?.User == null || string.IsNullOrEmpty(claimsViewModel.User.Id))
                {
                    TempData["Error"] = "Invalid user data";
                    return RedirectToAction(nameof(Index));
                }

                IdentityUser user = await _userManager.FindByIdAsync(claimsViewModel.User.Id);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                // Remove existing claims
                var oldClaims = await _userManager.GetClaimsAsync(user);
                var removeResult = await _userManager.RemoveClaimsAsync(user, oldClaims);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Error while removing existing claims: " +
                                        string.Join(", ", removeResult.Errors.Select(e => e.Description));
                    return await ManagerUserClaim(claimsViewModel.User.Id);
                }

                // Add selected claims
                var selectedClaims = claimsViewModel.ClaimList?
                    .Where(x => x.IsSelected)
                    .Select(y => new Claim(y.ClaimType, y.IsSelected.ToString()))
                    .ToList() ?? new List<Claim>();

                if (selectedClaims.Any())
                {
                    var addResult = await _userManager.AddClaimsAsync(user, selectedClaims);
                    if (!addResult.Succeeded)
                    {
                        TempData["Error"] = "Error while adding new claims: " +
                                            string.Join(", ", addResult.Errors.Select(e => e.Description));
                        return await ManagerUserClaim(claimsViewModel.User.Id);
                    }
                }

                TempData["Success"] = "Claims assigned successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error managing claims: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User ID is required";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
                {
                    // Unlock user
                    user.LockoutEnd = DateTime.Now;
                    TempData["Success"] = "User unlocked successfully";
                }
                else
                {
                    // Lock user
                    user.LockoutEnd = DateTime.Now.AddYears(100);
                    TempData["Success"] = "User locked successfully";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating user lock status: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "User ID is required";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                // Optional: Check if user has important data before deletion
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Contains(SD.Role_Pharmacist) || userRoles.Contains(SD.Role_PharmacyManager))
                {
                    TempData["Error"] = "Cannot delete user with pharmacist or manager role. Please remove roles first.";
                    return RedirectToAction(nameof(Index));
                }

                _db.ApplicationUsers.Remove(user);
                await _db.SaveChangesAsync();

                TempData["Success"] = "User deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}