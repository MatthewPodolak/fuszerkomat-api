using fuszerkomat_api.Data;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static fuszerkomat_api.Helpers.DomainExceptions;

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
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] WorkTaskFilterVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            if (!ModelState.IsValid)
            {
                throw new ValidationException(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            var res = await _workTaskService.GetWorkTasksAsync(model, userId, ct);
            return Ok(res);
        }
        
        [HttpGet("get-by-id")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result<CompanyWorkTaskVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromQuery] int id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = await _workTaskService.GetWorkTaskForCompanyAsync(id, userId, ct);
            return Ok(res);
        }

        [HttpGet("get-applied")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result<List<CompanyTaskApplyVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetApplied([FromQuery] CompanyAppliedFilterVM filter, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = await _workTaskService.GetCompanyAppliedTasksAsync(filter, userId, ct);
            return Ok(res);
        }


        [HttpPost("apply")]
        [Authorize(Policy = "CompanyOnly")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Apply([FromBody] ApplyToWorkTaskVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = await _workTaskService.ApplyForWorkTaskAsync(model, userId, ct);
            return Ok(res);
        }
    }
}
