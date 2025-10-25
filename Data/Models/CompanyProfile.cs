using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class CompanyProfile
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public int RealizedTasks { get; set; } = 0;
        public string? Desc { get; set; }
        public string? Nip { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; } = "/company/profiles/base-img.png";
        public string? BackgroundImg { get; set; } = "/company/backgrounds/base-background.png";
        public Address Address { get; set; } = default!;

        public string AppUserId { get; set; } = default!;
        public AppUser AppUser { get; set; } = default!;

        public ICollection<Opinion> Opinions { get; set; } = new List<Opinion>();
        public ICollection<Realization> Realizations { get; set; } = new List<Realization>();
    }
}
