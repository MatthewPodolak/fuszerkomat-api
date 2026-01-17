using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VM;

namespace fuszerkomat_api.VMO
{
    public class CompanyWorkTaskVMO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
        public decimal MaxPrice { get; set; } = 0m;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; } 
        public CategoryType Category { get; set; }
        public List<TagType> Tags { get; set; } = new List<TagType>();
        public RealisationTime ExpectedRealisationTime { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public Location? Location { get; set; }
        public bool Aplicated { get; set; } = false;
        public WorkTaskRequestingUserDataVMO RequestingUserDataVMO { get; set; }
        public List<ApplicantDataVMO> Applicants { get; set; } = new List<ApplicantDataVMO>();
    }

    public class WorkTaskRequestingUserDataVMO
    {
        public string? Name {  set; get; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Pfp { get; set; }
        public string PublicKey { get; set; }
    }

    public class ApplicantDataVMO
    {
        public string Id { set; get; }
        public string Name { get; set; }
        public string Pfp { get; set; }
    }
}
