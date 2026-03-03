using ChatApp.Interfaces;
using ChatApp.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
       private readonly IConversationService conversationService;
       private readonly JWT jwt;
        public ChatController(IConversationService conversationService,JWT jwt)
        {
            this.conversationService = conversationService;
            this.jwt = jwt;
        }

        [HttpPost("get-or-create-conversation")]
        public async Task<int> CreateOrGetConversation([FromQuery] int otherUserId)
        {
            int currentUserId = await jwt.GetUserID();
            var response=await conversationService.CreateOrGetConversation(currentUserId, otherUserId); 
            return response;    
        }
        [HttpPost("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromQuery]int conversationId)
        {
            await conversationService.MarkConversationAsRead(conversationId);
            return Ok();
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId,int pageNumber = 1,int pageSize = 20)
        {
            var result = await conversationService
                .GetConversationMessages(conversationId, pageNumber, pageSize);

            return Ok(result);
        }
    }
}
