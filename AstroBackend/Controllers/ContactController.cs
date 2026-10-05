using AstroBackend.Application.DTOs;
using AstroBackend.Application.Interfaces.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

/// <summary>"Əlaqə" formu — ictimai, lakin [EnableRateLimiting] ilə spam qarşısı alınır.</summary>
public class ContactController : BaseApiController
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpPost]
    [EnableRateLimiting("contact")]
    public async Task<ActionResult<ContactMessageDto>> Submit([FromBody] CreateContactMessageRequest request, CancellationToken ct)
    {
        var message = await _contactService.SubmitAsync(request, ct);
        return Ok(message);
    }
}
