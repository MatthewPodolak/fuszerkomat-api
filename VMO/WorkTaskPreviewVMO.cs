using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VM;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VMO
{
    public class WorkTaskPreviewVMO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal? MaxPrice { get; set; } = 0m;
        public int ReamainingDays { get; set; }
        public int Applicants { get; set; }
        public CategoryType Category { get; set; }
        public List<TagType> Tags { get; set; } = new List<TagType>();
        public WorkTaskPreviewLocationVMO? Location { get; set; }

        public WorkTaskRequestingUserDataPreviewVMO WorkTaskRequestingUserData { get; set; }
    }

    public class WorkTaskPreviewLocationVMO
    {
        public string? Name { get; set; }
        public double Longtitude { get; set; }
        public double Latitude { get; set; }
        public double? DistanceFrom { get; set; }
    }

    public class WorkTaskRequestingUserDataPreviewVMO
    {
        public string Name { get; set; }
        public string Pfp { get; set; }
    }
}