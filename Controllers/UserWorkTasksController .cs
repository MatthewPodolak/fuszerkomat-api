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
    public class UserWorkTasksController : Controller
    {
        private readonly IWorkTaskService _workTaskService;
        public UserWorkTasksController(IWorkTaskService workTaskService)
        {
            _workTaskService = workTaskService;
        }

        [HttpGet("get-by-id")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.GetWorkTaskForUserAsync(id, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpGet("get-own")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<List<UserWorkTaskPreviewVMO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOwn([FromQuery] OwnWorkTasksFilterVM filters, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result<List<UserWorkTaskPreviewVMO>>.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.GetOwnAsync(filters, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("publish")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Publish([FromForm] PublishWorkTaskVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(Result.BadRequest(errors: ModelState.Values.SelectMany(a => a.Errors).Select(e => new Error(ErrorCode.ValidationFailed, e.ErrorMessage)).ToList(), traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.PublishAsync(model, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPatch("change-application-status")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeApplicationStatus([FromBody] ChangeApplicationStatusVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.ChangeApplicationStatusAsync(model, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPatch("complete-realization")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CompleteRealization([FromBody] CompleteRealizationVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.CompleteRealization(model, userId, ct);
            return StatusCode(res.Status, res);
        }
    }
}
