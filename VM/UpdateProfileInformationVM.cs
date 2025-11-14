namespace fuszerkomat_api.VM
{
    public class UpdateProfileInformationVM
    {
        public UserProfileInfoVM? UserProfileInfo { get; set; }
        public CompanyProfileInfoVM? CompanyProfileInfo { get; set; }
    }

    public class UserProfileInfoVM
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? Photo { get; set; }
    }

    public class CompanyProfileInfoVM
    {
        public string? CompanyName { get; set; }
        public string? Desc { get; set; }
        public string? Nip { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? Photo { get; set; }
        public IFormFile? BackgroundPhoto { get; set; }
        public AdressVM? Adress { get; set; }
        public List<RealizationVM> NewRealizations { get; set; } = new List<RealizationVM>();
        public List<int> RelaizationsToDelete { get; set; } = new List<int>();
    }

    public class AdressVM
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public double? Longtitude { get; set; }
        public double? Lattitude { get; set; }
    }

    public class RealizationVM
    {
        public string Title { get; set; }
        public string Desc { get; set; }
        public IFormFile Img { get; set; }
        public DateOnly? Date { get; set; }
        public string? Localization { get; set; }
    }
}
