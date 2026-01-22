using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [Route("api/deck")]
    [ApiController]
    public class DeckController : ControllerBase
    {
        private readonly IDeckServices _deckServices;

        public DeckController(IDeckServices deckServices)
        {
            _deckServices = deckServices;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateDeck([FromBody] DTO.Request.AddNewDeckRequest request)
        {
            var result = await _deckServices.CreateDeckAsync(request);
            
            return result.IsSuccess
                ? StatusCode(result.StatusCode, new
                {
                    success = true,
                    message = result.Message,
                })
                : StatusCode(result.StatusCode, new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
        }
    }
}
