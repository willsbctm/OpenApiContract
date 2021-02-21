using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenApiContract.Validator.Api.Models;

namespace OpenApiContract.Validator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PetController : ControllerBase
    {
        private const string petUrl = "https://petstore.swagger.io/v2/pet";
        private readonly HttpClient client;
        private readonly ILogger<PetController> logger;

        public PetController(IHttpClientFactory factory, ILogger<PetController> logger)
        {
            this.client = factory.CreateClient("pet");
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Pet>> Get()
        {
            logger.LogInformation($"Calling {petUrl}");

            var pet = await client.GetAsync($"{petUrl}/3");

            return Ok(pet);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Pet pet)
        {
            logger.LogInformation($"Calling {petUrl}");

            var content = new StringContent(JsonSerializer.Serialize(pet, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }), Encoding.UTF8, "application/json");
            await client.PostAsync(petUrl, content);

            return Ok();
        }
    }
}
