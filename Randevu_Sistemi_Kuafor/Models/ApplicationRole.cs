using Microsoft.AspNetCore.Identity;

namespace Randevu_Sistemi_Kuafor.Models
{
    public class ApplicationRole : IdentityRole
    {
        // Varsayılan yapıcı (constructor)
        public ApplicationRole() : base() { }

        // Parametreli yapıcı
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}
