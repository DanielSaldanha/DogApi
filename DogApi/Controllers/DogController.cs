using DogApi.Model;
using Microsoft.AspNetCore.Mvc;

namespace DogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DogController : ControllerBase
    {
        private readonly IHttpClientFactory _IHttpClientFactory;
        public DogController(IHttpClientFactory ihttpclientfactory)
        {
            _IHttpClientFactory = ihttpclientfactory;
        }
        [HttpGet("pegue um cão")]
        public async Task<ActionResult> DogerGet()
        {
            var client = _IHttpClientFactory.CreateClient("CurrencyAPI");
            var response = await client.GetAsync($"breeds/list/all");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound("nao funcionou");
            }
            var DogJson = await response.Content.ReadFromJsonAsync<DogModel>();
            if(DogJson == null)
            {
                return NotFound("sem chances");
            }
            return Ok(DogJson);
        }
    }
}
