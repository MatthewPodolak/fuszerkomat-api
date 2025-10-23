using fuszerkomat_api.Data.Models;

namespace fuszerkomat_api.VM
{
    public class PublishWorkTaskVM
    {
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public decimal MaxPrice { get; set; } = 0m;
        public RealisationTime ExpectedRealisationTime { get; set; }
        public string? Location { get; set; }
        public List<IFormFile>? Images { get; set; }
        public CategoryType CategoryType { get; set; }
        public List<TagType> Tags { get; set; } = new List<TagType>();
    }
}
