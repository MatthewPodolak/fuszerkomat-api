using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;

namespace fuszerkomat_api.Services
{
    public class OpinionService : IOpinionService
    {
        private readonly IRepository<WorkTask> _workTaskRepo;
        private readonly IRepository<Opinion> _opinionRepo;
        private readonly IUnitOfWork _uow;

        private readonly ILogger<IOpinionService> _logger;
        private readonly IHttpContextAccessor _http;

        public OpinionService(IRepository<WorkTask> workTaskRepo, IRepository<Opinion> opinionRepo, IUnitOfWork uow, ILogger<IOpinionService> logger, IHttpContextAccessor http)
        {
            _workTaskRepo = workTaskRepo;
            _opinionRepo = opinionRepo;
            _uow = uow;
            _logger = logger;
            _http = http;
        }

        public async Task<Result<List<CompanyToRatePreviewVMO>>> GetAll(OpinionFiltersVM filters, string userId, CancellationToken ct)
        {
            try
            {
                int page = filters.PageNumber <= 0 ? 1 : filters.PageNumber;
                int pageSize = filters.PageSize <= 0 ? 10 : filters.PageSize;

                var workTasks = await _workTaskRepo.Query().AsNoTracking()
                    .Where(t => t.CreatedByUserId == userId && t.Status == Status.Completed)
                    .Include(t => t.Applications)
                        .ThenInclude(ap => ap.CompanyUser)
                            .ThenInclude(u => u.CompanyProfile)
                    .Include(t => t.Category)
                    .Include(t => t.Tags)
                    .Include(t => t.Opinion)
                    .ToListAsync(ct);


                var projected = workTasks.Select(t =>
                {
                    var accepted = t.Applications.FirstOrDefault(ap => ap.Status == ApplicationStatus.Accepted);
                    var profile = accepted?.CompanyUser?.CompanyProfile;
                    if (accepted == null || profile == null) return null;

                    var opinion = t.Opinion;

                    return new CompanyToRatePreviewVMO
                    {
                        CompanyId = accepted.CompanyUserId,
                        CompanyName = profile.CompanyName,
                        CompanyPfp = profile.Img,

                        TaskData = new CompanyToRateTaskDataVMO
                        {
                            TaskId = t.Id,
                            Name = t.Name,
                            Desc = t.Desc,
                            Category = t.Category.CategoryType,
                            Tags = t.Tags.Select(a => a.TagType).ToList(),
                        },

                        CompanyRating = opinion == null
                            ? null
                            : new CompanyRateVMO
                            {
                                Comment = opinion.Comment,
                                Rating = Convert.ToDouble(opinion.Rating),
                                CreatedAt = opinion.CreatedAt
                            }
                    };
                })
                .Where(v => v != null)!.Cast<CompanyToRatePreviewVMO>().ToList();

                IEnumerable<CompanyToRatePreviewVMO> filtered = projected;
                if (filters.Types != null && filters.Types.Any())
                {
                    bool includeRated = filters.Types.Contains(OpinionType.Rated);
                    bool includeNotRated = filters.Types.Contains(OpinionType.NotRated);

                    if (includeRated && !includeNotRated)
                    {
                        filtered = filtered.Where(x => x.CompanyRating != null);
                    }
                    else if (!includeRated && includeNotRated)
                    {
                        filtered = filtered.Where(x => x.CompanyRating == null);
                    }
                }

                var totalCount = filtered.Count();
                var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
                var pagedData = filtered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagination = new Pagination
                {
                    CurrentPage = page,
                    Count = pagedData.Count,
                    PageCount = pageCount,
                    TotalCount = totalCount
                };

                return Result<List<CompanyToRatePreviewVMO>>.Ok(data: pagedData, pagination: pagination, traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "GetAll was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<CompanyToRatePreviewVMO>>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<CompanyToRatePreviewVMO>>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }

        public async Task<Result> RateCompany(RateCompanyVM model, string userId, CancellationToken ct)
        {
            string traceId = _http.HttpContext?.TraceIdentifier ?? string.Empty;

            try
            {
                var workTask = await _workTaskRepo.Query().Include(a => a.Applications).Include(d => d.Opinion)
                    .Where(a=>a.Status == Status.Completed && a.CreatedByUserId == userId).FirstOrDefaultAsync(a => a.Id == model.TaskId, ct);
                    
                if(workTask == null)
                {
                    return Result.NotFound(errors: null, traceId: traceId);
                }

                if(workTask.Opinion != null)
                {
                    return Result.Conflict(errors: new List<Error>() { Error.Conflict(msg: "Already rated") }, traceId: traceId);
                }

                var hasAcceptedApplicationForCompany = workTask.Applications.Any(a => a.CompanyUserId == model.CompanyId && a.Status == ApplicationStatus.Accepted);
                if (!hasAcceptedApplicationForCompany)
                {
                    _logger.LogWarning("User is trying to rate company not accepted for given task. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                    return Result.NotFound(errors: null, traceId: traceId);
                }

                var opinion = new Opinion()
                {
                    AuthorUserId = userId,
                    Comment = model.Comment,
                    CompanyId = model.CompanyId,
                    CreatedAt = DateTime.UtcNow,
                    InternalOpinion = true,
                    Rating = model.Rating,
                    WorkTaskId = model.TaskId
                };

                _opinionRepo.Add(opinion);
                await _uow.SaveChangesAsync(ct);

                return Result.Ok(errors: null, traceId: traceId);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "PublishAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Canceled(errors: null, traceId: traceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result.Internal(errors: null, traceId: traceId);
            }
        }
    }
}
