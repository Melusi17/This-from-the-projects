using IbhayiPharmacy.Data;
using IbhayiPharmacy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IbhayiPharmacy.Controllers
{
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleController(ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var roles = _db.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Upsert(string roleId)
        {
            if (String.IsNullOrEmpty(roleId))
            {
                return View();
            }
            else
            {
                var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == roleId);
                if (objFromDb == null)
                {
                    TempData["Error"] = "Role not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(objFromDb);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole roleObj)
        {
            if (await _roleManager.RoleExistsAsync(roleObj.Name))
            {
                TempData["Error"] = $"Role '{roleObj.Name}' already exists";
                return View(roleObj);
            }

            if (String.IsNullOrEmpty(roleObj.Id))
            {
                // Create new role
                var result = await _roleManager.CreateAsync(new IdentityRole() { Name = roleObj.Name });
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role created successfully";
                }
                else
                {
                    TempData["Error"] = "Error creating role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            else
            {
                // Update existing role
                var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == roleObj.Id);
                if (objFromDb == null)
                {
                    TempData["Error"] = "Role not found";
                    return RedirectToAction(nameof(Index));
                }

                objFromDb.Name = roleObj.Name;
                objFromDb.NormalizedName = roleObj.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(objFromDb);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Role updated successfully";
                }
                else
                {
                    TempData["Error"] = "Error updating role: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "OnlySuperAdminChecker")]
        public async Task<IActionResult> Delete(string roleId)
        {
            var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == roleId);
            if (objFromDb == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deletion of essential roles
            var essentialRoles = new[] { SD.Role_Customer, SD.Role_Pharmacist, SD.Role_PharmacyManager };
            if (essentialRoles.Contains(objFromDb.Name))
            {
                TempData["Error"] = $"Cannot delete essential role '{objFromDb.Name}'";
                return RedirectToAction(nameof(Index));
            }

            var userRolesForThisRole = _db.UserRoles.Where(u => u.RoleId == roleId).Count();
            if (userRolesForThisRole > 0)
            {
                TempData["Error"] = "Cannot delete this role, since there are users assigned to this role.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(objFromDb);
            if (result.Succeeded)
            {
                TempData["Success"] = "Role deleted successfully";
            }
            else
            {
                TempData["Error"] = "Error deleting role: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        // Optional: Method to initialize standard roles
        public async Task<IActionResult> InitializeStandardRoles()
        {
            var standardRoles = new[]
            {
                SD.Role_Customer,
                SD.Role_Pharmacist,
                SD.Role_PharmacyManager
            };

            foreach (var roleName in standardRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            TempData["Success"] = "Standard roles initialized successfully";
            return RedirectToAction(nameof(Index));
        }

        // Optional: Method to get users in a role
        public async Task<IActionResult> UsersInRole(string roleId)
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _db.UserRoles.Where(ur => ur.RoleId == roleId).ToListAsync();
            var userIds = userRoles.Select(ur => ur.UserId).ToList();
            var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            ViewBag.RoleName = role.Name;
            return View(users);
        }
    }
}