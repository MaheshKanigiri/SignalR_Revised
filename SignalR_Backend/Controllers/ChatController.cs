using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR_Backend.Models;
using System;
using System.Threading.Tasks;

namespace SignalR_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] MessageRequest request)
        {
            string clientId = request.ClientId;
            string data = request.Data;

            _ = AlertCompletion(clientId, data);

            await _hubContext.Clients.All.SendAsync("SendMessage", data);

            return Ok(new { message = "Request processed, you will receive an update soon." });
        }

        private async Task AlertCompletion(string clientId, string msgData)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            var response = new MessageResponse
            {
                Data = msgData,
                SentDateTime = DateTime.UtcNow
            };

            await _hubContext.Clients.Client(clientId).SendAsync("ReceiveMessage", response);
        }

        [HttpPost("sendGroupMessage")]
        public async Task<IActionResult> SendGroupMessage([FromBody] GroupMessageRequest request)
        {
            var response = new GroupMessageResponse
            {
                GroupName = request.GroupName,
                Message = request.Message,
                SentDateTime = DateTime.UtcNow,
                Sender = request.Sender
            };

            await _hubContext.Clients.Group(request.GroupName).SendAsync("ReceiveGroupMessage", response);
            return Ok(new { message = "Group message sent.", response });
        }

        [HttpPost("addToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] GroupRequest request)
        {
            await _hubContext.Groups.AddToGroupAsync(request.ConnectionId, request.GroupName);
            return Ok(new { message = "User added to group." });
        }

        [HttpPost("removeFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] GroupRequest request)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(request.ConnectionId, request.GroupName);
            return Ok(new { message = "User removed from group." });
        }
    }

    public class MessageRequest
    {
        public string ClientId { get; set; }
        public string Data { get; set; }
    }

    public class MessageResponse
    {
        public string Data { get; set; }
        public DateTime SentDateTime { get; set; }
    }

    public class GroupMessageRequest
    {
        public string GroupName { get; set; }
        public string Message { get; set; }
        public string Sender { get; set; }
    }

    public class GroupMessageResponse
    {
        public string GroupName { get; set; }
        public string Message { get; set; }
        public DateTime SentDateTime { get; set; }
        public string Sender { get; set; }
    }

    public class GroupRequest
    {
        public string ConnectionId { get; set; }
        public string GroupName { get; set; }
    }
}
