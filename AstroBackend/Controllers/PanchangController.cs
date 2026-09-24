using Microsoft.AspNetCore.Mvc;

namespace AstroBackend.Controllers
{
    public class PanchangController : BaseApiController
    {
        private readonly IPanchangService _panchangService;

        public PanchangController(IPanchangService panchangService)
        {
            _panchangService = panchangService;
        }

        [HttpGet("today")]
        public ActionResult<PanchangResponse> GetToday([FromQuery] double lat = 40.4093, [FromQuery] double lon = 49.8671)
        {
            var response = _panchangService.GetPanchang(new PanchangRequest(DateTime.UtcNow, lat, lon));
            return Ok(response);
        }

        [HttpPost("calculate")]
        public ActionResult<PanchangResponse> Calculate([FromBody] PanchangRequest request)
        {
            var response = _panchangService.GetPanchang(request);
            return Ok(response);
        }
    }

}
