using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Surname { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; } = "/users/base-img.png";


        public string AppUserId { get; set; } = default!;
        public AppUser AppUser { get; set; } = default!;
        public ICollection<Opinion> OpinionsAuthored { get; set; } = new List<Opinion>();
    }
}
