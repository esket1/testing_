using Microsoft.AspNetCore.Mvc;
using CalculatorApi.Models;

namespace CalculatorApi.Controllers
{
    [ApiController]
    [Route("api/calculator")]
    public class CalculatorController : ControllerBase
    {
        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] CalculatorRequest request)
        {
            if (request.Days <= 0) return BadRequest("Количество дней должно быть больше нуля.");

            var total = request.DailyCost * request.Days;

            // Баг: страховка должна стоить 5%, а сейчас умножается на 5 (500%)
            if (request.IncludeInsurance)
            {
                total *=1.05m;
                // total *= 5;
            }

            return Ok(new { TotalAmount = total });
        }
    }
}