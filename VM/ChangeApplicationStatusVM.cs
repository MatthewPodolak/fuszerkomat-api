using fuszerkomat_api.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class ChangeApplicationStatusVM
    {
        public int WorkTaskId { get; set; }

        [Required]
        public string CompanyId { get; set; }

        [Required]
        public AnswerAplication Answer { get; set; }
    }

    public enum AnswerAplication
    {
        Accept,
        Reject
    }
}
