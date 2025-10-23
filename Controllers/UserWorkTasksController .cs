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
    }
}
