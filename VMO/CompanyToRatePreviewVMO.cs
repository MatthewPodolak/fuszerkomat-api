using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VMO
{
    public class CompanyToRatePreviewVMO
    {
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPfp { get; set; }

        public CompanyToRateTaskDataVMO TaskData { get; set; }
        public CompanyRateVMO? CompanyRating { get; set; }
    }

    public class CompanyToRateTaskDataVMO
    {
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public List<string>? Imgs { get; set; }
    }

    public class CompanyRateVMO
    {
        public string? Comment { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
