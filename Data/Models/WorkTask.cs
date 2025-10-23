using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class WorkTask
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal MaxPrice { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(31);
        public RealisationTime ExpectedRealisationTime { get; set; }
        public string Location { get; set; }
        public double Lattitude { get; set; }
        public double Longttitude { get; set; }
        public Status Status { get; set; }

        public string CreatedByUserId { get; set; } = default!;
        public AppUser CreatedByUser { get; set; } = default!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public ICollection<WorkTaskGallery> Images { get; set; } = new List<WorkTaskGallery>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<TaskApplication> Applications { get; set; } = new List<TaskApplication>();
    }

    public enum RealisationTime
    {
        Asap,
        ThisWeek,
        TwoWeeks,
        Adaptive
    }
    public enum Status
    {
        Open,
        Closed,
        Completed,
        Canceled
    }

}
