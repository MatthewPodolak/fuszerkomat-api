using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.Data.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; }
        public double Lattitude { get; set; } = 52.2297;
        public double Longtitude { get; set; } = 21.0122;

        public int CompanyProfileId { get; set; } = default!;
        public CompanyProfile CompanyProfile { get; set; } = default!;
    }
}
