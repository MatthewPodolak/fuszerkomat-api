using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
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
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("get-company-profile")]
        [Authorize]
        [ProducesResponseType(typeof(Result<CompanyProfileVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<CompanyProfileVMO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<CompanyProfileVMO>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result<CompanyProfileVMO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanyProfile([FromQuery] string id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _accountService.GetCompanyProfileAsync(id, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPatch("/ProfileInfromation")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProfileInformation([FromForm] UpdateProfileInformationVM model, CancellationToken ct)
        {
            var accountType = User.FindFirstValue("account_type");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(accountType) || String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = new Result();

            switch (accountType)
            {
                case "User":
                    if(model.UserProfileInfo == null) { return BadRequest(Result.BadRequest(new List<Error>() { Error.Validation(msg: "User profile missing. Nothing to update.") }, traceId: HttpContext.TraceIdentifier)); }
                    res = await _accountService.UpdateUserInformation(userId, model.UserProfileInfo, ct);
                    break;
                case "Company":
                    if (model.CompanyProfileInfo == null) { return BadRequest(Result.BadRequest(new List<Error>() { Error.Validation(msg: "Company profile missing. Nothing to update.") }, traceId: HttpContext.TraceIdentifier)); }
                    res = await _accountService.UpdateCompanyInfrormation(userId, model.CompanyProfileInfo, ct);
                    break;
                default:
                    return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            return StatusCode(res.Status, res);
        }

        [HttpDelete("DeleteAccount")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccount(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _accountService.DeleteAccount(userId, ct);
            return StatusCode(res.Status, res);
        }
    }
}
