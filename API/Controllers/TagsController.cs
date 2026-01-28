using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace API.Controllers
{
    [EnableCors("AllowClient")]
    [Route("api/tag")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagsServices _tagsServices;

        public TagsController(ITagsServices tagsServices)
        {
            _tagsServices = tagsServices;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNewTag([FromQuery] string name)
        {
            var result = await _tagsServices.AddNewTagAsync(name);

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
