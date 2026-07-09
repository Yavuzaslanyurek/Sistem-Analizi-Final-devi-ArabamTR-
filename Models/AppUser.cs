using Microsoft.AspNetCore.Identity;

namespace ArabamTR.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Kullanıcının yüklediği araç ilanları ile ilişkisi
        public ICollection<VehicleAd> VehicleAds { get; set; }
    }
}
