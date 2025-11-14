namespace fuszerkomat_api.VMO
{
    public class ProfileVMO
    {
        public OwnCompanyProfileDataVMO? CompanyProfileDataVMO { get; set; }
        public OwnUserProfileDataVMO? UserProfileDataVMO { get; set; }
    }

    public class OwnCompanyProfileDataVMO
    {
        public string CompanyName { get; set; }
        public int RealizedTasks { get; set; }
        public int OpinionCount { get; set; }
        public double Rate { get; set; }
        public string? Desc { get; set; }
        public string? Nip { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; }
        public string? BackgroundImg { get; set; }
        public AdressVMO? Adress { get; set; }
        public List<OpinionVMO> Opinions { get; set; } = new List<OpinionVMO>();
        public List<RealizationVMO> Realizations { get; set; } = new List<RealizationVMO>();
    }

    public class OwnUserProfileDataVMO
    {
        public string Name { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Img { get; set; }
    }
}
