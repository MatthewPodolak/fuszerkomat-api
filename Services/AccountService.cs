using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly ILogger<IAccountService> _logger;
        private readonly IHttpContextAccessor _http;
        public AccountService(IRepository<AppUser> userRepo, ILogger<IAccountService> logger, IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _logger = logger;
            _http = http;
        }

        public async Task<Result> UpdateCompanyInfrormation(string userId, CompanyProfileInfoVM model, CancellationToken ct)
        {
            try
            {
                var user = await _userRepo.Query().Include(a => a.CompanyProfile).ThenInclude(a => a.Address).FirstOrDefaultAsync(a => a.Id == userId);
                if(user == null)
                {
                    _logger.LogInformation("UpdateCompanyInfrormation couldnt find user with given id. Path={Path} Method={Method} UserId={UserId}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method, userId);
                    return Result.NotFound(new List<Error>() { Error.NotFound(msg: "User with given id not found.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }
                if (user.AccountType != AccountType.Company)
                {
                    return Result.BadRequest([Error.Validation("Only company accounts can update company profile.")], traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var newCompanyInfo = user.CompanyProfile ?? new CompanyProfile() { AppUserId = user.Id, AppUser = user };

                if (model.CompanyName != null) newCompanyInfo.CompanyName = model.CompanyName;
                if (model.Desc != null) newCompanyInfo.Desc = model.Desc;
                if (model.Nip != null) newCompanyInfo.Nip = model.Nip;
                if (model.Email != null) newCompanyInfo.Email = model.Email;
                if (model.PhoneNumber != null) newCompanyInfo.PhoneNumber = model.PhoneNumber;

                if(model.Adress != null)
                {
                    newCompanyInfo.Address ??= new Address();

                    if (model.Adress.Street != null) newCompanyInfo.Address.Street = model.Adress.Street;
                    if (model.Adress.City != null) newCompanyInfo.Address.City = model.Adress.City;
                    if (model.Adress.PostalCode != null) newCompanyInfo.Address.PostalCode = model.Adress.PostalCode;
                    if (model.Adress.Country != null) newCompanyInfo.Address.Country = model.Adress.Country;
                }

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    newCompanyInfo.Img = await PhotoSaver.SavePhotoAsync(
                        model.Photo,
                        physicalFolder: Path.Combine("Assets", "Images", "Company", "Profile"),
                        requestPathPrefix: "/company/profiles",
                        ct);
                }

                if (model.BackgroundPhoto != null && model.BackgroundPhoto.Length > 0)
                {
                    newCompanyInfo.BackgroundImg = await PhotoSaver.SavePhotoAsync(
                        model.BackgroundPhoto,
                        physicalFolder: Path.Combine("Assets", "Images", "Company", "Backgrounds"),
                        requestPathPrefix: "/company/backgrounds",
                        ct);
                }


                if (user.CompanyProfile == null)
                    user.CompanyProfile = newCompanyInfo;

                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync(ct);

                return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "UpdateCompanyInfrormation was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateCompanyInfrormation unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result> UpdateUserInformation(string userId, UserProfileInfoVM model, CancellationToken ct)
        {
            try
            {
                var user = await _userRepo.Query().Include(a => a.UserProfile).FirstOrDefaultAsync(a => a.Id == userId);
                if (user == null)
                {
                    _logger.LogInformation("UpdateUserInformation couldnt find user with given id. Path={Path} Method={Method} UserId={UserId}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method, userId);
                    return Result.NotFound(new List<Error>() { Error.NotFound(msg: "User with given id not found.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }
                if (user.AccountType != AccountType.User)
                {
                    return Result.BadRequest([Error.Validation("Only user accounts can update user profile.")], traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var profile = user.UserProfile ?? new UserProfile { AppUserId = user.Id };

                if (model.Name != null) profile.Name = model.Name;
                if (model.Surname != null) profile.Surname = model.Surname;
                if (model.Email != null) profile.Email = model.Email;
                if (model.PhoneNumber != null) profile.PhoneNumber = model.PhoneNumber;

                if (model.Photo != null && model.Photo.Length > 0)
                {
                    profile.Img = await PhotoSaver.SavePhotoAsync(
                        file: model.Photo,
                        physicalFolder: Path.Combine("Assets", "Images", "Users"),
                        requestPathPrefix: "/users",
                        ct: ct);
                }

                if (user.UserProfile is null)
                    user.UserProfile = profile;

                _userRepo.Update(user);
                await _userRepo.SaveChangesAsync(ct);

                return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "UpdateUserInformation was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateUserInformation unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
