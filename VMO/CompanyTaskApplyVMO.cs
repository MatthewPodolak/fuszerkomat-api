using fuszerkomat_api.Data.Models;
using fuszerkomat_api.VM;

namespace fuszerkomat_api.VMO
{
    public class CompanyTaskApplyVMO
    {
        public int TaskId { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public decimal? MaxPrice { get; set; } = 0m;
        public string? Location { get; set; }
        public ApplicationStatus Status { get; set; }
        public CategoryType Category { get; set; }
        public List<TagType> Tags { get; set; } = new List<TagType>();
    }
}
