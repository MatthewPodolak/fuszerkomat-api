using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VM;

namespace fuszerkomat_api.VMO
{
    public class UserWorkTaskPreviewVMO
    {
        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal? MaxPrice { get; set; } = 0m;
        public int ReamainingDays { get; set; }
        public int Applicants { get; set; }
        public Location? Location { get; set; }
        public Status Status { get; set; }
    }
}
