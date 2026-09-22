using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
