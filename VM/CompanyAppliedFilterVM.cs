using fuszerkomat_api.Data.Models;

namespace fuszerkomat_api.VM
{
    public class CompanyAppliedFilterVM
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public List<ApplicationStatus>? Statuses { get; set; }
    }
}
