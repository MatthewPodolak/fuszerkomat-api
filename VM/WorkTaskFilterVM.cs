using fuszerkomat_api.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VM
{
    public class WorkTaskFilterVM
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 50;

        public string? KeyWords { get; set; }
        public SortOptions? SortOptions { get; set; }
        public CategoryType? CategoryType { get; set; }
        public List<TagType>? Tags { get; set; }
        public Location? Location { get; set; }
    }

    public class Location
    {
        public string? Name { get; set; }
        public double Longtitude {  get; set; }
        public double Latitude { get; set; }
        public int Range { get; set; } = 100;
    }

    public enum SortOptions
    {
        Nearest,
        LowestApplicants,
        MaxPriceAsc,
        MaxPriceDesc,
        DeadlineAsc,
        DeadlineDesc,
    }
}