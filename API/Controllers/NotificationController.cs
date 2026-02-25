using API.Payloads;
using Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("notifications")]
    public class NotificationController : Controller
    {
        private readonly INotificationsService _notificationsService;

        public NotificationController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [HttpPost("recod/token/{userId}/{token}")]
        public async Task<IActionResult> RecordToken(string userId, string token)
        {
            try
            {
                var parsedUser = Guid.TryParse(userId, out Guid parsedUserId);

                if (!parsedUser)
                    throw new Exception("Informe o Id corretamente");

                await _notificationsService.RecordUserToken(parsedUserId, token);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("markasread/{userNotificationId}")]
        public async Task<IActionResult> MarkAsRead(string userNotificationId)
        {
            try
            {
                var parsedUserNotificationId = Guid.TryParse(userNotificationId, out Guid parsedId);

                if (!parsedUserNotificationId)
                    throw new Exception("Informe o Id corretamente");

                await _notificationsService.MarkAsRead(parsedId);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("send/to/all")]
        public async Task<IActionResult> SendToAll([FromBody] NotificationPayload payload)
        {
            try
            {
                await _notificationsService.SendToAll(payload.Title,
                    payload.Body, payload.Data);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("send/to/someones")]
        public async Task<IActionResult> SendToSomeones([FromBody] NotificationPayload payload)
        {
            try
            {
                if(payload.UsersId == null)
                    throw new Exception("Lista de usuários inválidas");

                List<Guid> ids = [];
                foreach (var userIds in payload.UsersId)
                {
                    var parsedUserNotificationId = Guid.TryParse(userIds, out Guid parsedId);

                    if (!parsedUserNotificationId)
                        continue;

                    ids.Add(parsedId);
                }

                await _notificationsService.SendToSomeUsers(
                    ids,
                    payload.Title,
                    payload.Body, payload.Data);

                return Ok();
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
