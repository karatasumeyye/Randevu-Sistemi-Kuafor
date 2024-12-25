using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Randevu_Sistemi_Kuafor.Models
{
    public class ApplicationUser:IdentityUser
    {
        // User tablosundaki özel alanlar buraya taşınır
        [Required]
        [Display(Name = "Full Name")]
        public String Name { get; set; } // Eski "Name" alanı
    }

}


// IdentityUser zaten Email, PhoneNumber, ve PasswordHash gibi alanları içerir.
// Bu nedenle Email ve Phone gibi alanları tekrar tanımlamanıza gerek yok.
