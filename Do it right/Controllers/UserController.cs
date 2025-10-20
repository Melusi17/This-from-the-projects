using IbhayiPharmacy.Data;
using IbhayiPharmacy.Models;
using IbhayiPharmacy.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace IbhayiPharmacy.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManage)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManage;

        }
        public async Task<IActionResult> Index()
        {
            var userList = _db.ApplicationUsers.ToList();


            foreach (var user in userList)
            {
                var user_role = await _userManager.GetRolesAsync(user) as List<string>;
                user.Role = String.Join(",", user_role);

                var user_Claims = _userManager.GetClaimsAsync(user).GetAwaiter().GetResult().Select(u => u.Type);
                user.UserClaim = String.Join(",", user_Claims);
            }
            return View(userList);
        }
        public async Task<IActionResult> ManagerRole(string userId)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            List<string> existingUserRoles = await _userManager.GetRolesAsync(user) as List<string>;
            var model = new RolesViewModel()
            {
                User = user
            };

            foreach (var role in _roleManager.Roles)
            {
                RoleSelection roleSelection = new()
                {

                    RoleName = role.Name
                };
                if (existingUserRoles.Any(c => c == role.Name))
                {
                    roleSelection.IsSelected = true;
                }
                model.RoleList.Add(roleSelection);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerRole(RolesViewModel rolesViewModel)
        {
            IdentityUser user = await _userManager.FindByIdAsync(rolesViewModel.User.Id);
            if (user == null)
            {
                return NotFound();
            }

            var oldRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, oldRoles);
            //if (!result.Succeeded)
            //{
            //    TempData[SD.Error] = "Error while removing roles";
            //    return View(rolesViewModel);
            //}


            result = await _userManager.AddToRolesAsync(user,
                rolesViewModel.RoleList.Where(x => x.IsSelected).Select(y => y.RoleName));

            //if (!result.Succeeded)
            //{
            //    TempData[SD.Error] = "Error while adding roles";
            //    return View(rolesViewModel);
            //}

            //TempData[SD.Success] = "Roles assighn successfully";
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> ManagerUserClaim(string userId)
        {
            IdentityUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
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

                    ClaimType = claim.Type
                };
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                model.ClaimList.Add(userClaim);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagerUserClaim(ClaimsViewModel claimsViewModel)
        {
            IdentityUser user = await _userManager.FindByIdAsync(claimsViewModel.User.Id);
            if (user == null)
            {
                return NotFound();
            }

            var oldClaims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, oldClaims);
            //if (!result.Succeeded)
            //{
            //    TempData[SD.Error] = "Error while removing claims";
            //    return View(claimsViewModel);
            //}


            result = await _userManager.AddClaimsAsync(user,
                claimsViewModel.ClaimList.Where(x => x.IsSelected).Select(y => new Claim(y.ClaimType, y.IsSelected.ToString())));

            //if (!result.Succeeded)
            //{
            //    TempData[SD.Error] = "Error while adding claims";
            //    return View(claimsViewModel);
            //}

            //TempData[SD.Success] = "Claims assighn successfully";
            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUnlock(string userId)
        {
            ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {

                user.LockoutEnd = DateTime.Now;
                // TempData[SD.Success] = "User unlocked successfully";
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
                // TempData[SD.Success] = "User locked successfully";
            }
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            _db.ApplicationUsers.Remove(user);
            _db.SaveChanges();
            // TempData[SD.Success] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
