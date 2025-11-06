using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Grpc;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace fuszerkomat_api.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<WorkTask> _workTaskRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Data.Models.Tag> _tagRepo;
        private readonly IUnitOfWork _uow;
        private readonly Chat.ChatClient _chatClient;

        private readonly ILogger<IWorkTaskService> _logger;
        private readonly IHttpContextAccessor _http;

        public WorkTaskService(IRepository<AppUser> userRepo, IRepository<WorkTask> workTaskRepo, IRepository<Category> categoryRepo, IRepository<Data.Models.Tag> tagRepo, IUnitOfWork uow, Chat.ChatClient chatClient, ILogger<IWorkTaskService> logger, IHttpContextAccessor http)
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

                if (model.Location != null)
                {
                    newWorkTask.Lattitude = Convert.ToDouble(model.Location.Latitude, CultureInfo.InvariantCulture);
                    newWorkTask.Longttitude = Convert.ToDouble(model.Location.Longtitude, CultureInfo.InvariantCulture);
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

        public async Task<Result<List<WorkTaskPreviewVMO>>> GetWorkTasksAsync(WorkTaskFilterVM filter, string userId, CancellationToken ct)
        {
            try
            {
                var askingUser = await _userRepo.Query().Include(a=>a.CompanyProfile).ThenInclude(ac=>ac.Address).FirstOrDefaultAsync(a => a.Id == userId, ct);
                if (askingUser == null)
                {
                    _logger.LogInformation("PublishAsync couldnt find user with given id. Path={Path} Method={Method} UserId={UserId}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method, userId);
                    return Result<List<WorkTaskPreviewVMO>>.NotFound(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var query = _workTaskRepo.Query().Where(a=>a.Status == Data.Models.Status.Open);

                if (filter.Tags != null && filter.Tags.Any())
                {
                    var tagSet = filter.Tags.ToHashSet();
                    query = query.Where(a => a.Tags.Any(t => tagSet.Contains(t.TagType)));
                }

                if (filter.CategoryType != null)
                {
                    query = query.Where(a => a.Category.CategoryType == filter.CategoryType);
                }

                query = query.Include(a => a.Tags).Include(a => a.Category).Include(a=>a.CreatedByUser).ThenInclude(a=>a.UserProfile);

                if (!String.IsNullOrEmpty(filter.KeyWords))
                {
                    query = query.Where(a => a.Desc!.Contains(filter.KeyWords) || a.Name.Contains(filter.KeyWords));
                }

                if(filter.Location != null)
                {
                    var taskCoords = await query.Select(w => new { w.Id, w.Lattitude, w.Longttitude }).ToListAsync(ct);

                    var nearbyIds = taskCoords
                        .Where(w => GeoUtils.DistanceKm(filter.Location.Latitude, filter.Location.Longtitude, w.Lattitude, w.Longttitude) <= filter.Location.Range)
                        .Select(w => w.Id)
                        .ToList();

                    query = query.Where(w => nearbyIds.Contains(w.Id));

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
                    Id = a.Id,
                    Desc = a.Desc,
                    MaxPrice = a.MaxPrice,
                    Name = a.Name,
                    Category = a.Category.CategoryType,
                    Tags = a.Tags.Select(b => b.TagType).ToList(),
                    Applicants = a.Applications.Count,
                    ReamainingDays = Convert.ToInt16((a.ExpiresAt - DateTime.Now).TotalDays),
                    Location = new WorkTaskPreviewLocationVMO() { Latitude = a.Lattitude, Longtitude = a.Longttitude, Name = a.LocationName },
                    WorkTaskRequestingUserData = new WorkTaskRequestingUserDataPreviewVMO() { Name = a.CreatedByUser.UserProfile.Name, Pfp = a.CreatedByUser.UserProfile.Img },
                })
                .ToListAsync(ct);

                var userLat = askingUser?.CompanyProfile?.Address?.Lattitude;
                var userLon = askingUser?.CompanyProfile?.Address?.Longtitude;

                if (userLat.HasValue && userLon.HasValue)
                {
                    foreach (var item in res)
                    {
                        var lat = item.Location?.Latitude;
                        var lon = item.Location?.Longtitude;

                        if (lat.HasValue && lon.HasValue && item.Location != null)
                        {
                            item.Location.DistanceFrom = GeoUtils.GetDistanceBetween(userLat.Value, userLon.Value, lat.Value, lon.Value);
                        }
                        else
                        {
                            item.Location!.DistanceFrom = null;
                        }
                    }
                }

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
                    Category = workTask.Category.CategoryType,
                    Tags = workTask.Tags.Select(a=>a.TagType).ToList(),
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

                if(workTask.Status != Data.Models.Status.Open)
                {
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

        public async Task<Result<List<UserWorkTaskPreviewVMO>>> GetOwnAsync(OwnWorkTasksFilterVM filters, string userId, CancellationToken ct)
        {
            try
            {
                var query = _workTaskRepo.Query().Include(t => t.Applications).Where(t => t.CreatedByUserId == userId);

                if (filters.Statuses is { Count: > 0 })
                {
                    query = query.Where(t => filters.Statuses.Contains(t.Status));
                }

                var page = filters.Page <= 0 ? 1 : filters.Page;
                var pageSize = filters.PageSize <= 0 ? 10 : Math.Min(filters.PageSize, 100);

                var totalCount = await query.CountAsync(ct);
                var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

                var skip = (page - 1) * pageSize;

                var res = await query.OrderByDescending(t => t.CreatedAt).Skip(skip).Take(pageSize).ToListAsync(ct);
                var vmo = res.Select(a => new UserWorkTaskPreviewVMO()
                {
                    Desc = a.Desc,
                    MaxPrice = a.MaxPrice,
                    Name = a.Name,
                    Applicants = a.Applications.Count,
                    ReamainingDays = Convert.ToInt16((a.ExpiresAt - DateTime.Now).TotalDays),
                    Location = new Location() { Latitude = a.Lattitude, Longtitude = a.Longttitude, Name = a.Location },
                    Status = a.Status
                }).ToList();

                var pagination = new Pagination()
                {
                    CurrentPage = page,
                    Count = vmo.Count,
                    TotalCount = totalCount,
                    PageCount = pageCount
                };

                return Result<List<UserWorkTaskPreviewVMO>>.Ok(data: vmo, pagination: pagination, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetOwnAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<UserWorkTaskPreviewVMO>>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOwnAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<UserWorkTaskPreviewVMO>>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result> ChangeApplicationStatusAsync(ChangeApplicationStatusVM model, string userId, CancellationToken ct)
        {
            try
            {
                var workTask = await _workTaskRepo.Query().Include(a=>a.Applications)
                    .FirstOrDefaultAsync(a => a.CreatedByUserId == userId && a.Id == model.WorkTaskId, ct);

                if(workTask == null)
                {
                    _logger.LogWarning("ChangeApplicationStatusAsync tried to acess npn existing workTask. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.NotFound(errors: new List<Error>() { Error.NotFound(msg: "workTask does not exist.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var targetApplication = workTask.Applications.FirstOrDefault(a => a.CompanyUserId == model.CompanyId);
                if(targetApplication == null)
                {
                    return Result.NotFound(errors: new List<Error>() { Error.NotFound(msg: "This company didnt applicated for that task.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                switch (model.Answer)
                {
                    case AnswerAplication.Accept:
                        if(workTask.Applications.Any(a=>a.Status == ApplicationStatus.Accepted))
                        {
                            return Result.Conflict(errors: new List<Error>() { Error.Conflict(msg: "Another company has already been accepted.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                        }

                        targetApplication.Status = ApplicationStatus.Accepted;
                        workTask.Status = Data.Models.Status.Closed;

                        foreach (var app in workTask.Applications.Where(a => a.CompanyUserId != model.CompanyId))
                        {
                            if (app.Status != ApplicationStatus.Rejected)
                                app.Status = ApplicationStatus.Rejected;
                        }
                        break;
                    case AnswerAplication.Reject:
                        targetApplication.Status = ApplicationStatus.Rejected;
                        //TODO
                        //delete chat?
                        //notify company.
                        break;
                }

                await _uow.SaveChangesAsync(ct);
                return Result.Ok(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "ChangeApplicationStatusAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangeApplicationStatusAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result> CompleteRealization(CompleteRealizationVM model, string userId, CancellationToken ct)
        {
            try
            {
                var workTask = await _workTaskRepo.Query()
                    .Include(a=>a.Applications).ThenInclude(a=>a.CompanyUser).ThenInclude(ac=>ac.CompanyProfile)
                    .FirstOrDefaultAsync(a => a.Id == model.WorkTaskId, ct);

                if(workTask == null)
                {
                    _logger.LogWarning("CompleteRealization tried to acess npn existing workTask. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.NotFound(errors: new List<Error>() { Error.NotFound(msg: "workTask does not exist.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                var application = workTask.Applications.FirstOrDefault(a=>a.CompanyUserId == model.CompanyId && a.Status == ApplicationStatus.Accepted);
                if(application == null)
                {
                    return Result.NotFound(errors: new List<Error>() { Error.NotFound(msg: "Company didnt applied.") }, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
                }

                application.CompanyUser.CompanyProfile.RealizedTasks += 1;
                workTask.Status = Data.Models.Status.Completed;
                await _uow.SaveChangesAsync(ct);

                return Result.Ok(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "CompleteRealization was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CompleteRealization unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(errors: null, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
