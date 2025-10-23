using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fuszerkomat_api.Data.Models
{
    public class WorkTaskGallery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Img { get; set; } = "/tasks/base-task.png";

        [Required]
        public int WorkTaskId { get; set; }
        public WorkTask WorkTask { get; set; } = default!;
    }
}
