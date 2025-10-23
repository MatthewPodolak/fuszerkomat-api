using fuszerkomat_api.Data;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(Result<List<WorkTaskPreviewVMO>>.BadRequest(errors: ModelState.Values.SelectMany(a => a.Errors).Select(e => new Error(ErrorCode.ValidationFailed, e.ErrorMessage)).ToList(), traceId: HttpContext.TraceIdentifier));
            }

            var res = await _workTaskService.GetWorkTasksAsync(model, ct);
            return StatusCode(res.Status, res);
        }
    }
}
