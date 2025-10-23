using fuszerkomat_api.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace fuszerkomat_api.VMO
{
    public class CompanyProfileVMO
    {
        public string CompanyName { get; set; }
        public string? Desc { get; set; }
        public string? Nip { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; }
        public string? BackgroundImg { get; set; }
        public AdressVMO Adress { get; set; }
        public List<OpinionVMO> Opinions { get; set; } = new List<OpinionVMO>();
        public List<RealizationVMO> Realizations { get; set; } = new List<RealizationVMO>();
    }

    public class AdressVMO
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public double Lattitude { get; set; }
        public double Longtitude { get; set; }
    }

    public class OpinionVMO
    {
        public string? Comment { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByName { get; set; }
    }

    public class RealizationVMO
    {
        public string? Desc { get; set; }
        public string Img { get; set; }
        public DateOnly? Date { get; set; }
        public string? Localization { get; set; }
    }
}
