using fuszerkomat_api.Data;
using fuszerkomat_api.Data.Models;
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
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet("get-own-profile")]
        [Authorize]
        [ProducesResponseType(typeof(Result<ProfileVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOwnProfileData(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var accountType = User.FindFirstValue("account_type");
            if (String.IsNullOrEmpty(userId) || string.IsNullOrEmpty(accountType))
            {
                throw new UnauthorizedException();
            }

            var res = await _accountService.GetOwnProfileDataAsync(userId, accountType, ct);
            return Ok(res);
        }

        [HttpGet("get-company-profile")]
        [Authorize]
        [ProducesResponseType(typeof(Result<CompanyProfileVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCompanyProfile([FromQuery] string id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = await _accountService.GetCompanyProfileAsync(id, ct);
            return Ok(res);
        }

        [HttpPatch("profile-informaion")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProfileInformation([FromForm] UpdateProfileInformationVM model, CancellationToken ct)
        {
            var accountType = User.FindFirstValue("account_type");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(accountType) || String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = new Result();

            switch (accountType)
            {
                case "User":
                    if (model.UserProfileInfo == null) { throw new ValidationException("User profile missing. Nothing to update."); }
                    res = await _accountService.UpdateUserInformation(userId, model.UserProfileInfo, ct);
                    break;
                case "Company":
                    if (model.CompanyProfileInfo == null) { throw new ValidationException("Company profile missing. Nothing to update."); }
                    res = await _accountService.UpdateCompanyInfrormation(userId, model.CompanyProfileInfo, ct);
                    break;
                default:
                    throw new UnauthorizedException();
            }

            return Ok(res);
        }

        [HttpDelete("delete-account")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAccount(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException();
            }

            var res = await _accountService.DeleteAccount(userId, ct);
            return Ok(res);
        }
    }
}
