using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;

namespace fuszerkomat_api.Services
{
    public class WorkTaskService : IWorkTaskService
    {
        private readonly IRepository<AppUser> _userRepo;
        private readonly IRepository<WorkTask> _workTaskRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Tag> _tagRepo;
        private readonly IUnitOfWork _uow;

        private readonly ILogger<IWorkTaskService> _logger;
        private readonly IHttpContextAccessor _http;

        public WorkTaskService(IRepository<AppUser> userRepo, IRepository<WorkTask> workTaskRepo, IRepository<Category> categoryRepo, IRepository<Tag> tagRepo, IUnitOfWork uow, ILogger<IWorkTaskService> logger, IHttpContextAccessor http)
        {
            _userRepo = userRepo;
            _workTaskRepo = workTaskRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _uow = uow;
            _logger = logger;
            _http = http;
        }

        public async Task<Result> PublishAsync(PublishWorkTaskVM model, string userId, CancellationToken ct)
        {
            try
            {
                var user = await _userRepo.GetByIdAsync(userId);
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
                    Status = Status.Open,
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
                    WorkTaskRequestingUserData = new WorkTaskRequestingUserDataVMO() { Name = a.CreatedByUser.UserProfile.Name, Pfp = a.CreatedByUser.UserProfile.Img },
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
    }
}
