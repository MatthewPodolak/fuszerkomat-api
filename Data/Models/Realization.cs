using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Realization
    {
        [Key]
        public int Id { get; set; }

        public string? Desc { get; set; }
        public string Img { get; set; } = "/company/realizations/base-realization.png";
        public DateOnly? Date { get; set; }
        public string? Localization { get; set; }

        public int CompanyProfileId { get; set; } = default!;
        public CompanyProfile CompanyProfile { get; set; } = default!;
    }
}
