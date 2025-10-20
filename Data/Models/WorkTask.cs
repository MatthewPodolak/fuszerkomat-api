using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class WorkTask
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal Price { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
        public DateTime? Deadline {  get; set; }
        public string Location { get; set; }
        public Currency Currency { get; set; }
        public Status Status { get; set; }

        public string CreatedByUserId { get; set; } = default!;
        public AppUser CreatedByUser { get; set; } = default!;

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<TaskApplication> Applications { get; set; } = new List<TaskApplication>();
    }

    public enum Currency
    {
        PLN,
        EUR,
        USD
    }
    public enum Status
    {
        Open,
        Completed,
        Canceled
    }

}
