using fuszerkomat_api.Data;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.VM;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterVM model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                throw new ValidationException(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            var res = await _authService.RegisterAsync(model, ct);
            return Ok(res);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginVM model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                throw new ValidationException(string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            var res = await _authService.LoginAsync(model, ct);
            return Ok(res);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result<AuthTokenVMO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Refresh([FromBody] string refreshTokebn, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshTokebn))
            {
                throw new ValidationException();
            }

            var res = await _authService.RefreshAsync(refreshTokebn, ct);
            return Ok(res);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var res = await _authService.LogoutAsync(ct);
            return Ok(res);
        }
    }
}
