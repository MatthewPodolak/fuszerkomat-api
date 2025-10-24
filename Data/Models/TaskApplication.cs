using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class TaskApplication
    {
        [Key] 
        public int Id { get; set; }

        public int WorkTaskId { get; set; }
        public WorkTask WorkTask { get; set; } = default!;

        public string CompanyUserId { get; set; } = default!;
        public AppUser CompanyUser { get; set; } = default!;

        public string? Message { get; set; }
        public string ChatId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
    }

    public enum ApplicationStatus
    {
        Applied = 0,
        Rejected = 1,
        Accepted = 2
    }
}
