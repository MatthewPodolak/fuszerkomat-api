using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class RateCompanyVM
    {
        public string CompanyId { get; set; }
        public string? Comment { get; set; }

        [Range(1, 5)]
        public double Rating { get; set; }
    }
}
