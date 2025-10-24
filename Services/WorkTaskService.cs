using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Grpc;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<WorkTask> _workTaskRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Tag> _tagRepo;
        private readonly IUnitOfWork _uow;
        private readonly Chat.ChatClient _chatClient;

        private readonly ILogger<IWorkTaskService> _logger;
        private readonly IHttpContextAccessor _http;

        public WorkTaskService(IRepository<AppUser> userRepo, IRepository<WorkTask> workTaskRepo, IRepository<Category> categoryRepo, IRepository<Tag> tagRepo, IUnitOfWork uow, Chat.ChatClient chatClient, ILogger<IWorkTaskService> logger, IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _workTaskRepo = workTaskRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _uow = uow;
            _chatClient = chatClient;
            _logger = logger;
            _http = http;
        }

        public async Task<Result> PublishAsync(PublishWorkTaskVM model, string userId, CancellationToken ct)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId, ct);
                if (user == null)
                {
                    _logger.LogInformation("PublishAsync couldnt find user with given id. Path={Path} Method={Method} UserId={UserId}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method, userId);
                    return Result.NotFound(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var category = _categoryRepo.Query().FirstOrDefault(a => a.CategoryType == model.CategoryType);
                if(category == null)
                {
                    _logger.LogWarning("PublishAsync unexpected category. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.NotFound(new List<Error>() { Error.NotFound(msg: $"Category {model.CategoryType} does not exists.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var tags = _tagRepo.Query().Where(a=> model.Tags.Contains(a.TagType)).ToList();
                if(tags.Count != model.Tags.Count)
                {
                    _logger.LogWarning("PublishAsync one or more unexpected tags. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.NotFound(new List<Error>() { Error.NotFound(msg: "One or more tags are invalid.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                if(model.Images?.Count > 5)
                {
                    _logger.LogWarning("PublishAsync images upload limit exceeded. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.BadRequest(new List<Error>() { Error.Validation(msg: "Max 5 images allowed.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var newWorkTask = new WorkTask()
                {
                    CreatedByUser = user,
                    CreatedByUserId = userId,
                    Name = model.Name ?? "Zlecenie " + new Random().Next(0, 9999),
                    Desc = model.Desc,
                    ExpectedRealisationTime = model.ExpectedRealisationTime,
                    Status = Data.Models.Status.Open,
                    MaxPrice = model.MaxPrice,
                    Category = category,
                    CategoryId = category.Id,
                    Tags = tags,
                };

                if(model.Location != null)
                {
                    newWorkTask.Lattitude = model.Location.Latitude;
                    newWorkTask.Longttitude = model.Location.Longtitude;
                    newWorkTask.Location = model.Location.Name ?? string.Empty;
                }
                else
                {
                    newWorkTask.Location = "Polska";
                    newWorkTask.Lattitude = 0;
                    newWorkTask.Longttitude = 0;
                }

                    foreach (var img in model.Images)
                    {
                        string path = await PhotoSaver.SavePhotoAsync
                            (
                                img,
                                physicalFolder: Path.Combine("Assets", "Images", "WorkTasks"),
                                requestPathPrefix: "/tasks",
                                ct
                            );

                        newWorkTask.Images.Add(new WorkTaskGallery
                        {
                            Img = path
                        });
                    }

                await _workTaskRepo.AddAsync(newWorkTask, ct);
                await _uow.SaveChangesAsync(ct);

                return Result.Ok(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "PublishAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<List<WorkTaskPreviewVMO>>> GetWorkTasksAsync(WorkTaskFilterVM filter, CancellationToken ct)
        {
            try
            {
                var query = _workTaskRepo.Query();

                if (filter.Tags != null && filter.Tags.Any())
                {
                    var tagSet = filter.Tags.ToHashSet();
                    query = query.Where(a => a.Tags.Any(t => tagSet.Contains(t.TagType)));
                }

                if (filter.CategoryType != null)
                {
                    query.Where(a => a.Category.CategoryType == filter.CategoryType);
                }

                query = query.Include(a => a.Tags).Include(a => a.Category).Include(a=>a.CreatedByUser).ThenInclude(a=>a.UserProfile);

                if (!String.IsNullOrEmpty(filter.KeyWords))
                {
                    query.Where(a => a.Desc!.Contains(filter.KeyWords) || a.Name.Contains(filter.KeyWords));
                }

                if(filter.Location != null)
                {
                    query.Where(w => GeoUtils.DistanceKm(filter.Location.Latitude, filter.Location.Longtitude, w.Lattitude, w.Longttitude) <= filter.Location.Range);
                }

                var totalCount = await query.CountAsync(ct);

                //TODO add nearest (loc.) sort.
                switch (filter.SortOptions)
                {
                    case SortOptions.LowestApplicants:
                        query = query.OrderBy(a => a.Applications.Count);
                        break;

                    case SortOptions.MaxPriceAsc:
                        query = query.OrderBy(a => a.MaxPrice);
                        break;

                    case SortOptions.MaxPriceDesc:
                        query = query.OrderByDescending(a => a.MaxPrice);
                        break;

                    case SortOptions.DeadlineAsc:
                        query = query.OrderBy(a => a.ExpiresAt);
                        break;

                    case SortOptions.DeadlineDesc:
                        query = query.OrderByDescending(a => a.ExpiresAt);
                        break;
                }


                var res = await query.Skip((filter.PageSize * (filter.Page - 1))).Take(filter.PageSize).Select(a => new WorkTaskPreviewVMO
                {
                    Desc = a.Desc,
                    MaxPrice = a.MaxPrice,
                    Name = a.Name,
                    Applicants = a.Applications.Count,
                    ReamainingDays = Convert.ToInt16((a.ExpiresAt - DateTime.Now).TotalDays),
                    Location = new Location() { Latitude = a.Lattitude, Longtitude = a.Longttitude, Name = a.Location },
                    WorkTaskRequestingUserData = new WorkTaskRequestingUserDataPreviewVMO() { Name = a.CreatedByUser.UserProfile.Name, Pfp = a.CreatedByUser.UserProfile.Img },
                })
                .ToListAsync(ct);

                var pageCount = (int)Math.Ceiling((double)totalCount / filter.PageSize);
                var pagination = new Pagination
                {
                    Count = res.Count,
                    CurrentPage = filter.Page,
                    PageCount = pageCount,
                    TotalCount = totalCount
                };

                return Result<List<WorkTaskPreviewVMO>>.Ok(data: res, pagination: pagination, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetWorkTasksAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<WorkTaskPreviewVMO>>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWorkTasksAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<WorkTaskPreviewVMO>>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<UserWorkTaskVMO>> GetWorkTaskForUserAsync(int id, string userId, CancellationToken ct)
        {
            try
            {
                var workTask = await _workTaskRepo.Query()
                            .AsNoTracking()
                            .Include(a => a.CreatedByUser).ThenInclude(u => u.UserProfile)
                            .Include(a => a.Applications).ThenInclude(ap => ap.CompanyUser).ThenInclude(cu => cu.CompanyProfile)
                            .Include(a => a.Category)
                            .Include(a => a.Tags)
                            .Include(a => a.Images)
                            .FirstOrDefaultAsync(a => a.Id == id, ct);

                if (workTask == null)
                {
                    _logger.LogWarning("GetWorkTaskForUserAsync tried to acess non existing worktask. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<UserWorkTaskVMO>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var vmo = new UserWorkTaskVMO
                {
                    Id = workTask.Id,
                    CreatedAt = workTask.CreatedAt,
                    ExpiresAt = workTask.ExpiresAt,
                    Desc = workTask.Desc,
                    MaxPrice = workTask.MaxPrice,
                    Name = workTask.Name,
                    ExpectedRealisationTime = workTask.ExpectedRealisationTime,
                    Own = workTask.CreatedByUserId == userId,
                    RequestingUserDataVMO = new WorkTaskRequestingUserDataVMO
                    {
                        Name = workTask.CreatedByUser.UserProfile?.Name,
                        Email = workTask.CreatedByUser.UserProfile?.Email,
                        Phone = workTask.CreatedByUser.UserProfile?.PhoneNumber ?? string.Empty,
                        Pfp = workTask.CreatedByUser.UserProfile?.Img
                    },
                    Applicants = workTask.Applications.Select(a => new ApplicantDataVMO
                    {
                        Id = a.CompanyUser.Id,
                        Name = a.CompanyUser.CompanyProfile.CompanyName,
                        Pfp = a.CompanyUser.CompanyProfile.Img
                    }).ToList(),
                    Images = workTask.Images.Any() ? workTask.Images.Select(i => i.Img).ToList() : new List<string>(),
                    Location = new Location
                    {
                        Name = workTask.Location,
                        Latitude = workTask.Lattitude,
                        Longtitude = workTask.Longttitude
                    }
                };

                return Result<UserWorkTaskVMO>.Ok(data: vmo, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetWorkTaskForUserAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<UserWorkTaskVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWorkTaskForUserAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<UserWorkTaskVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<CompanyWorkTaskVMO>> GetWorkTaskForCompanyAsync(int id, string userId, CancellationToken ct)
        {
            try
            {
                var workTask = await _workTaskRepo.Query()
                            .AsNoTracking()
                            .Include(a => a.CreatedByUser).ThenInclude(u => u.UserProfile)
                            .Include(a => a.Applications).ThenInclude(ap => ap.CompanyUser).ThenInclude(cu => cu.CompanyProfile)
                            .Include(a => a.Category)
                            .Include(a => a.Tags)
                            .Include(a => a.Images)
                            .FirstOrDefaultAsync(a => a.Id == id, ct);

                if (workTask == null)
                {
                    _logger.LogWarning("GetWorkTaskForCompanyAsync tried to acess non existing worktask. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<CompanyWorkTaskVMO>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var vmo = new CompanyWorkTaskVMO
                {
                    Id = workTask.Id,
                    CreatedAt = workTask.CreatedAt,
                    ExpiresAt = workTask.ExpiresAt,
                    Desc = workTask.Desc,
                    MaxPrice = workTask.MaxPrice,
                    Name = workTask.Name,
                    ExpectedRealisationTime = workTask.ExpectedRealisationTime,
                    Aplicated = workTask.Applications.Any(a => a.CompanyUserId == userId),
                    RequestingUserDataVMO = new WorkTaskRequestingUserDataVMO
                    {
                        Name = workTask.CreatedByUser.UserProfile?.Name,
                        Email = workTask.CreatedByUser.UserProfile?.Email,
                        Phone = workTask.CreatedByUser.UserProfile?.PhoneNumber ?? string.Empty,
                        Pfp = workTask.CreatedByUser.UserProfile?.Img
                    },
                    Applicants = workTask.Applications.Select(a => new ApplicantDataVMO
                    {
                        Id = a.CompanyUser.Id,
                        Name = a.CompanyUser.CompanyProfile.CompanyName,
                        Pfp = a.CompanyUser.CompanyProfile.Img
                    }).ToList(),
                    Images = workTask.Images.Any() ? workTask.Images.Select(i => i.Img).ToList() : new List<string>(),
                    Location = new Location
                    {
                        Name = workTask.Location,
                        Latitude = workTask.Lattitude,
                        Longtitude = workTask.Longttitude
                    }
                };

                return Result<CompanyWorkTaskVMO>.Ok(data: vmo, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetWorkTaskForCompanyAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<CompanyWorkTaskVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetWorkTaskForCompanyAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<CompanyWorkTaskVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result<ApplyVMO>> ApplyForWorkTaskAsync(ApplyToWorkTaskVM model, string companyId, CancellationToken ct)
        {
            try
            {
                var workTask = await _workTaskRepo.Query().Include(a => a.CreatedByUser).Include(a => a.Applications).ThenInclude(ap => ap.CompanyUser).FirstOrDefaultAsync(a => a.Id == model.WorkTaskId, ct);
                if (workTask == null)
                {
                    _logger.LogWarning("ApplyForWorkTaskAsync tried to acess non existing worktask. Id={Id} Path={Path}, Method={Method}", model.WorkTaskId, _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<ApplyVMO>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                if(workTask.Applications.Any(a=>a.CompanyUserId == companyId))
                {
                    _logger.LogWarning("ApplyForWorkTaskAsync doubled application. Id={Id} CompanyId={CompanyId} Path={Path}, Method={Method}", model.WorkTaskId, companyId, _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result<ApplyVMO>.Conflict(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                OpenOrGetConversationResponse chatResp;
                try
                {
                    var deadline = DateTime.UtcNow.AddSeconds(10);

                    chatResp = await _chatClient.OpenOrGetConversationAsync(
                        new OpenOrGetConversationRequest
                        {
                            TaskId = workTask.Id,
                            OwnerUserId = workTask.CreatedByUserId,
                            CompanyUserId = companyId,
                            InitialMessage = model.InitialMessage ?? string.Empty
                        },
                        deadline: deadline,
                        cancellationToken: ct);
                }
                catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.NotFound)
                {
                    return Result<ApplyVMO>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }
                catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.InvalidArgument)
                {
                    return Result<ApplyVMO>.BadRequest(errors: new List<Error>() { Error.Unauthorized(msg: "gRPC failed to validate new conv.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }
                catch (RpcException rpcEx)
                {
                    _logger.LogError(rpcEx, "Chat OpenOrGetConversation failed for WorkTask {Id}", workTask.Id);
                    return Result<ApplyVMO>.Internal(errors: new List<Error>() { Error.Internal(msg: "gRPC failed to created new conv.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var newApplication = new TaskApplication()
                {
                    ChatId = chatResp.ConversationId,
                    CompanyUserId = companyId,
                    CreatedAtUtc = DateTime.UtcNow,
                    Message = model.InitialMessage,
                    Status = ApplicationStatus.Applied,
                    WorkTaskId = workTask.Id,
                };

                workTask.Applications.Add(newApplication);
                await _uow.SaveChangesAsync(ct);

                var vmo = new ApplyVMO() { ConversationId = chatResp.ConversationId };
                return Result<ApplyVMO>.Ok(data: vmo, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "ApplyForWorkTaskAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<ApplyVMO>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApplyForWorkTaskAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<ApplyVMO>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
