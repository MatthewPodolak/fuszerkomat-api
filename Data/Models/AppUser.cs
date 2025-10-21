using fuszerkomat_api.Data.Models.Token;
using Microsoft.AspNetCore.Identity;

namespace fuszerkomat_api.Data.Models
{
    public class AppUser : IdentityUser
    {
        public AccountType AccountType { get; set; }
        public UserProfile? UserProfile { get; set; }
        public CompanyProfile? CompanyProfile { get; set; }
        public ICollection<Opinion> Opinions { get; set; } = new List<Opinion>();
        public ICollection<WorkTask> CreatedTasks { get; set; } = new List<WorkTask>();
        public ICollection<TaskApplication> CompanyApplications { get; set; } = new List<TaskApplication>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }

    public enum AccountType
    {
        User,
        Company
    }
}
