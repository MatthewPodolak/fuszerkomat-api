using fuszerkomat_api.Data;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fuszerkomat_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyWorkTaskController : Controller
    {
        private readonly IWorkTaskService _workTaskService;
        public CompanyWorkTaskController(IWorkTaskService workTaskService)
        {
            _workTaskService = workTaskService;
        }

        [HttpGet("get-all")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result<List<WorkTaskPreviewVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<WorkTaskPreviewVMO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<List<WorkTaskPreviewVMO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<WorkTaskPreviewVMO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<List<WorkTaskPreviewVMO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] WorkTaskFilterVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Result<List<WorkTaskPreviewVMO>>.BadRequest(errors: ModelState.Values.SelectMany(a => a.Errors).Select(e => new Error(ErrorCode.ValidationFailed, e.ErrorMessage)).ToList(), traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.GetWorkTasksAsync(model, userId, ct);
            return StatusCode(res.Status, res);
        }
        
        [HttpGet("get-by-id")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result<CompanyWorkTaskVMO>.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.GetWorkTaskForCompanyAsync(id, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("apply")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Apply([FromBody] ApplyToWorkTaskVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.ApplyForWorkTaskAsync(model, userId, ct);
            return StatusCode(res.Status, res);
        }
    }
}
