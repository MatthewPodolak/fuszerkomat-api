using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Opinion
    {
        [Key]
        public int Id { get; set; }
        public string? Comment { get; set; }

        [Range(1,5)]
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool InternalOpinion { get; set; } = true;
        public string CompanyId { get; set; } = default!;
        public AppUser CompanyUser { get; set; } = default!;
        public string AuthorUserId { get; set; } = default!;
        public AppUser AuthorUser { get; set; } = default!;
    }
}
