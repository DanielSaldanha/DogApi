using DogApi.TokenService;
using Microsoft.AspNetCore.Mvc;

namespace DogApi.Controllers
{
    public class TokenAuth : ControllerBase
    {
        [HttpPost("insira")]
        public IActionResult SummonToken([FromBody] TKN props)
        {
            if(props.nome == "daniel" && props.senha == "123")
            {
                var token = TokenGenerator.GenerateToken(props.nome);
                return Ok(new { token });
            }
            return BadRequest("token não gerado");
        }
    }
    public class TKN
    {
        public string nome { get; set; }
        public string senha { get; set; }
    }
}
