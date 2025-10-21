using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
