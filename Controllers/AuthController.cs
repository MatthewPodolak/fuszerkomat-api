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
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterVM model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result<AuthTokenVMO>.BadRequest(errors: ModelState.Values.SelectMany(a => a.Errors).Select(e => new Error(ErrorCode.ValidationFailed, e.ErrorMessage)).ToList(), traceId: HttpContext.TraceIdentifier));
            }

            var res = await _authService.RegisterAsync(model, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginVM model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result<AuthTokenVMO>.BadRequest(errors: ModelState.Values.SelectMany(a => a.Errors).Select(e => new Error(ErrorCode.ValidationFailed, e.ErrorMessage)).ToList(), traceId: HttpContext.TraceIdentifier));
            }

            var res = await _authService.LoginAsync(model, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] string refreshTokebn, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshTokebn))
            {
                return BadRequest(Result<AuthTokenVMO>.BadRequest(traceId: HttpContext.TraceIdentifier));

            }

            var res = await _authService.RefreshAsync(refreshTokebn, ct);
            return StatusCode(res.Status, res);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var res = await _authService.LogoutAsync(ct);
            return StatusCode(res.Status, res);
        }
    }
}
