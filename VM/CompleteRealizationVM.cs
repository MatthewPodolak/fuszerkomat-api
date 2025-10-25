using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class CompleteRealizationVM
    {
        public int WorkTaskId { get; set; }

        [Required]
        public string CompanyId { get; set; }
    }
}
