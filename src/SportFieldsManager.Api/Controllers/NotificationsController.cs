using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SportFieldsManager.Api.Domain.Services;
using SportFieldsManager.Api.Models;

namespace SportFieldsManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailNotificationService _emailNotificationService;

    public NotificationsController(IEmailNotificationService emailNotificationService)
    {
        _emailNotificationService = emailNotificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), 200)]
    public async Task<ActionResult<IEnumerable<NotificationResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var notifications = await _emailNotificationService.GetNotificationsAsync(cancellationToken);
        var responses = notifications.Select(NotificationResponse.FromEntity).OrderByDescending(n => n.SentAt).ToList();
        return Ok(responses);
    }
}
