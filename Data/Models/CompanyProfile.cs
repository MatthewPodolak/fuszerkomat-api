using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class CompanyProfile
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Nip { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; } = "/company/base-img.png";
        public Address Address { get; set; } = default!;

        public string AppUserId { get; set; } = default!;
        public AppUser AppUser { get; set; } = default!;

        public ICollection<Opinion> Opinions { get; set; } = new List<Opinion>();
    }
}
