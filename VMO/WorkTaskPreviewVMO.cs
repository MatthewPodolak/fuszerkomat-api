using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VM;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VMO
{
    public class WorkTaskPreviewVMO
    {
        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal? MaxPrice { get; set; } = 0m;
        public int ReamainingDays { get; set; }
        public int Applicants { get; set; }
        public Location? Location { get; set; }

        public WorkTaskRequestingUserDataVMO WorkTaskRequestingUserData { get; set; }
    }

    public class WorkTaskRequestingUserDataVMO
    {
        public string Name { get; set; }
        public string Pfp { get; set; }
    }
}