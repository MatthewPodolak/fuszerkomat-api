using fuszerkomat_api.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class RegisterVM
    {
        [Required]
        public AccountType AccountType { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [PasswordPropertyText]
        [MinLength(7)]
        public string Password { get; set; }

        public string? Name { get; set; }
        public string? CompanyName { get; set; }
    }
}
