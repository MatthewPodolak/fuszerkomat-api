using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fuszerkomat_api.Data.Models.Token
{
    public class RefreshToken
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(128)]
        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public DateTime? LastUsedAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        public string UserId { get; set; } = default!;
        public AppUser User { get; set; } = default!;

        [NotMapped]
        public bool IsActive => RevokedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;
    }
}
