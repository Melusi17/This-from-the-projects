using Microsoft.AspNetCore.Identity;

namespace IbhayiPharmacy.Models.ViewModel
{
    public class RolesViewModel
    {
        public RolesViewModel()
        {
            RoleList = [];
        }
        public IdentityUser User { get; set; }
        public List<RoleSelection> RoleList { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
