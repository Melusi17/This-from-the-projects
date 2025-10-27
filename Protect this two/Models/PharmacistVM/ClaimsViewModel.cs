using Microsoft.AspNetCore.Identity;

namespace IbhayiPharmacy.Models.ViewModel
{
    public class ClaimsViewModel
    {
        public ClaimsViewModel()
        {
            ClaimList = [];
        }
        public IdentityUser User { get; set; }
        public List<ClaimSelection> ClaimList { get; set; }
    }

    public class ClaimSelection
    {
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }
}
