using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Helpers;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;

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
                    Location = model.Location ?? "Polska",
                    MaxPrice = model.MaxPrice,
                    Category = category,
                    CategoryId = category.Id,
                    Tags = tags,
                };

                foreach(var img in model.Images)
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
    }
}
