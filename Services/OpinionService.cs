using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.Repo;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
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

                var workTasks = await _workTaskRepo.Query().AsNoTracking().Where(t => t.CreatedByUserId == userId && t.Status == Status.Completed)
                    .Include(t => t.Images).Include(t => t.Applications).ThenInclude(ap => ap.CompanyUser).ThenInclude(u => u.CompanyProfile).ThenInclude(p => p.Opinions)
                    .ToListAsync(ct);

                var projected = workTasks
                .Select(t =>
                {
                    var accepted = t.Applications.FirstOrDefault(ap => ap.Status == ApplicationStatus.Accepted);
                    var profile = accepted?.CompanyUser?.CompanyProfile;
                    if (accepted == null || profile == null) return null;

                    var opinion = profile.Opinions?.FirstOrDefault(o => o.AuthorUserId == userId);

                    return new CompanyToRatePreviewVMO
                    {
                        CompanyId = accepted.CompanyUserId,
                        CompanyName = profile.CompanyName,
                        CompanyPfp = profile.Img,

                        TaskData = new CompanyToRateTaskDataVMO
                        {
                            Name = t.Name,
                            Desc = t.Desc,
                            Imgs = t.Images.Select(a => a.Img).ToList() ?? new List<string>()
                        },

                        CompanyRating = opinion == null ? null
                            : new CompanyRateVMO
                            {
                                Comment = opinion.Comment,
                                Rating = Convert.ToDouble(opinion.Rating),
                                CreatedAt = opinion.CreatedAt
                            }
                    };
                })
                .Where(v => v != null)!
                .ToList();

                var filtered = filters.Rated switch
                {
                    Rated.True => projected.Where(x => x.CompanyRating != null),
                    Rated.False => projected.Where(x => x.CompanyRating == null),
                    _ => projected
                };

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
                _logger.LogWarning(ex, "PublishAsync was canceled. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<CompanyToRatePreviewVMO>>.Canceled(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PublishAsync unexpected error. Path={Path}, Method={Method}", _http.HttpContext?.Request?.Path.Value, _http.HttpContext?.Request?.Method);
                return Result<List<CompanyToRatePreviewVMO>>.Internal(traceId: _http.HttpContext?.TraceIdentifier ?? string.Empty);
            }
        }
    }
}
