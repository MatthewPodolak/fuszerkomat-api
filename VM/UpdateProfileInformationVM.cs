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
    }

    public class AdressVM
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}
