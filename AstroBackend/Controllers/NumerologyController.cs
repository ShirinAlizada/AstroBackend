using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    public class NumerologyController : BaseApiController
    {
        private readonly INumerologyService _numerologyService;

        public NumerologyController(INumerologyService numerologyService)
        {
            _numerologyService = numerologyService;
        }

        [HttpPost("calculate")]
        public ActionResult<NumerologyResponse> Calculate([FromBody] NumerologyRequest request)
        {
            var response = _numerologyService.Calculate(request);
            return Ok(response);
        }
    }
}
