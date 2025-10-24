using fuszerkomat_api.Data;
using fuszerkomat_api.Interfaces;
using fuszerkomat_api.VMO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace fuszerkomat_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("get-chats")]
        [Authorize]
        [ProducesResponseType(typeof(Result<List<ChatVMO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<ChatVMO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<List<ChatVMO>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result<List<ChatVMO>>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChats(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (String.IsNullOrEmpty(userId))
            {
                return Unauthorized(Result<List<ChatVMO>>.Unauthorized(null, traceId: HttpContext.TraceIdentifier));
            }

            var res = await _chatService.GetChatsAsync(userId, ct);
            return StatusCode(res.Status, res);
        }
    }
}
