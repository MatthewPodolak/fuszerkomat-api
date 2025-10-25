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
    public class OpinionController : Controller
    {
        private readonly IOpinionService _opinionService;
        public OpinionController(IOpinionService opinionService)
        {
            _opinionService = opinionService;
        }

        [HttpGet("get-all-poss")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] OpinionFiltersVM filters, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _opinionService.GetAll(filters, userId, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("rate")]
        [Authorize(Policy = "UserOnly")]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RateCompany([FromBody] RateCompanyVM model, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _opinionService.RateCompany(model, userId, ct);
            return StatusCode(res.Status, res);
        }

        [AllowAnonymous]
        [HttpPost("outside-rate")]
        [ProducesResponseType(typeof(Result<List<CompanyToRatePreviewVMO>>), StatusCodes.Status501NotImplemented)]
        public async Task<IActionResult> RateCompanyFromDiffrentSource()
        {
            return StatusCode(StatusCodes.Status501NotImplemented); //COULD BE THO :) //TODO
        }
    }
}
