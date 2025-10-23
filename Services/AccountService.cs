using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Services
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<Opinion> _opinionRepo;
        private readonly IRepository<WorkTask> _taskRepo;
        private readonly IRepository<TaskApplication> _appRepo;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly ILogger<IAccountService> _logger;
        private readonly IHttpContextAccessor _http;
        public AccountService
            (
                IRepository<AppUser> userRepo,
                IRepository<Opinion> opinionRepo,
                IRepository<WorkTask> taskRepo,
                IRepository<TaskApplication> appRepo,
                IUnitOfWork uow, 
                ITokenService tokenService,
                ILogger<IAccountService> logger, 
                IHttpContextAccessor http
            )
        {
            _userRepo = userRepo;
            _taskRepo = taskRepo;
            _appRepo = appRepo;
            _opinionRepo = opinionRepo;
            _uow = uow;
            _tokenService = tokenService;
            _logger = logger;
            _http = http;
        }

        public async Task<Result<CompanyProfileVMO>> GetCompanyProfileAsync(string id, CancellationToken ct)
        {
            try
            {
                var companyData = await _userRepo.Query().AsNoTracking()
                    .Include(u => u.CompanyProfile)
                        .ThenInclude(cp => cp.Opinions).ThenInclude(cpo => cpo.AuthorUser).ThenInclude(cpop => cpop.UserProfile)
                    .Include(u => u.CompanyProfile)
                        .ThenInclude(cp => cp.Realizations).FirstOrDefaultAsync(u => u.Id == id, ct);

                if(companyData == null)
                {
                    _logger.LogWarning("GetCompanyProfileAsync couldnt find user with given id. Id={Id} Path={Path}, Method={Method}",id, _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<CompanyProfileVMO>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var vmo = new CompanyProfileVMO()
                {
                    Desc = companyData.CompanyProfile.Desc,
                    CompanyName = companyData.CompanyProfile.CompanyName,
                    Email = companyData.CompanyProfile.Email,
                    Img = companyData.CompanyProfile.Img,
                    Nip = companyData.CompanyProfile.Nip,
                    BackgroundImg = companyData.CompanyProfile.BackgroundImg,
                    PhoneNumber = companyData.CompanyProfile.PhoneNumber,
                    Adress = new AdressVMO()
                    {
                        City = companyData.CompanyProfile.Address.City,
                        Country = companyData.CompanyProfile.Address.Country,
                        Street = companyData.CompanyProfile.Address.Street,
                        Lattitude = companyData.CompanyProfile.Address.Lattitude,
                        Longtitude = companyData.CompanyProfile.Address.Lattitude,
                        PostalCode = companyData.CompanyProfile.Address.PostalCode
                    },
                    Opinions = companyData.Opinions.Select(a=> new OpinionVMO()
                    {
                        Comment = a.Comment,
                        CreatedAt = a.CreatedAt,
                        CreatedByName = a.AuthorUser.UserProfile.Name,
                        Rating = a.Rating
                    }).ToList(),
                    Realizations = companyData.CompanyProfile.Realizations.Select(a=> new RealizationVMO() 
                    { 
                        Date = a.Date,
                        Desc = a.Desc,
                        Img = a.Img,
                        Localization = a.Localization,
                    }).ToList()
                };

                return Result<CompanyProfileVMO>.Ok(data: vmo, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetCompanyProfileAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<CompanyProfileVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCompanyProfileAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<CompanyProfileVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
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
                await _uow.SaveChangesAsync(ct);

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
                await _uow.SaveChangesAsync(ct);

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

        public async Task<Result> DeleteAccount(string userId, CancellationToken ct)
        {
            var filesToDelete = new List<(string folder, string url)>();

            try
            {
                await _uow.ExecuteInTransactionAsync(async innerCt =>
                {
                    var user = await _userRepo.Query()
                        .Include(u => u.UserProfile)
                        .Include(u => u.CompanyProfile)!.ThenInclude(cp => cp.Realizations)
                        .FirstOrDefaultAsync(u => u.Id == userId, innerCt);

                    if (user is null)
                        throw new InvalidOperationException("NOT_FOUND_USER");

                    var ip = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
                    await _tokenService.RevokeAllForUserAsync(userId, ip, innerCt);

                    await _opinionRepo.Query().Where(o => o.AuthorUserId == userId).ExecuteDeleteAsync(innerCt);
                    await _opinionRepo.Query().Where(o => o.CompanyId == userId).ExecuteDeleteAsync(innerCt);
                    await _appRepo.Query().Where(a => a.CompanyUserId == userId).ExecuteDeleteAsync(innerCt);
                    await _taskRepo.Query().Where(t => t.CreatedByUserId == userId).ExecuteDeleteAsync(innerCt);

                    if (!string.IsNullOrWhiteSpace(user.UserProfile?.Img) &&
                        !string.Equals(user.UserProfile.Img, "/users/base-img.png", StringComparison.OrdinalIgnoreCase))
                    {
                        filesToDelete.Add(("Assets/Images/Users", user.UserProfile.Img));
                    }

                    if (user.CompanyProfile is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(user.CompanyProfile.Img) &&
                            !string.Equals(user.CompanyProfile.Img, "/company/profiles/base-img.png", StringComparison.OrdinalIgnoreCase))
                        {
                            filesToDelete.Add(("Assets/Images/Company/Profile", user.CompanyProfile.Img));
                        }

                        if (!string.IsNullOrWhiteSpace(user.CompanyProfile.BackgroundImg) &&
                            !string.Equals(user.CompanyProfile.BackgroundImg, "/company/backgrounds/base-background.png", StringComparison.OrdinalIgnoreCase))
                        {
                            filesToDelete.Add(("Assets/Images/Company/Backgrounds", user.CompanyProfile.BackgroundImg));
                        }

                        foreach (var r in user.CompanyProfile.Realizations)
                        {
                            if (!string.IsNullOrWhiteSpace(r.Img))
                                filesToDelete.Add(("Assets/Images/Company/Realizations", r.Img));
                        }
                    }

                    _userRepo.Delete(user);

                }, ct);

                foreach (var (folder, url) in filesToDelete)
                {
                    try
                    {
                        var fileName = Path.GetFileName(url);
                        var abs = Path.Combine(Directory.GetCurrentDirectory(), folder, fileName);
                        if (File.Exists(abs)) File.Delete(abs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file {Url}", url);
                    }
                }

                return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (InvalidOperationException ex) when (ex.Message == "NOT_FOUND_USER")
            {
                return Result.NotFound([Error.NotFound("User not found")],
                    traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "DeleteAccount was canceled. Path={Path}, Method={Method}",
                    _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAccount unhandled error. Path={Path}, Method={Method}",
                    _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
