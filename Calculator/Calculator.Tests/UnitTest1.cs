using Xunit;
using Microsoft.AspNetCore.Mvc;
using CalculatorApi.Controllers;
using CalculatorApi.Models;

namespace Calculator.Tests
{
    public class CalculatorControllerTests
    {
        private static decimal GetTotal(IActionResult result)
        {
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);

            var value = ok.Value!
                .GetType()
                .GetProperty("TotalAmount")!
                .GetValue(ok.Value);

            return Assert.IsType<decimal>(value);
        }

        private static IActionResult Run(decimal dailyCost, int days, bool insurance)
        {
            var controller = new CalculatorController();
            var request = new CalculatorRequest
            {
                DailyCost = dailyCost,
                Days = days,
                IncludeInsurance = insurance
            };
            return controller.Calculate(request);
        }

        // ---------- Часть 1: Happy Path ----------

        [Fact]
        public void Calculate_WithoutInsurance_ReturnsOkAndCorrectAmount()
        {
            var result = Run(100, 5, false);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(500m, GetTotal(result));
        }

        // ---------- Часть 2: Поиск бага (страховка) ----------

        // Если в контроллере стоит total *= 5, получим 2500 и тест упадёт
        [Fact]
        public void Calculate_WithInsurance_AddsFivePercent()
        {
            var result = Run(100, 5, true);

            Assert.Equal(525m, GetTotal(result)); // 500 * 1.05
        }

        // ---------- Негативные сценарии ----------

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void Calculate_NonPositiveDays_ReturnsBadRequest(int days)
        {
            var result = Run(100, days, false);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Calculate_ZeroDailyCost_ReturnsZero()
        {
            var result = Run(0, 5, false);

            Assert.Equal(0m, GetTotal(result));
        }

        // ---------- Округление ----------

        // 100.50 * 3 = 301.50
        [Fact]
        public void Calculate_DecimalDailyCost_CalculatesToKopecks()
        {
            var result = Run(100.50m, 3, false);

            Assert.Equal(301.50m, GetTotal(result));
        }

        // 100.50 * 3 * 1.05 = 316.575 (контроллер не округляет, decimal считает точно)
        [Fact]
        public void Calculate_DecimalWithInsurance_IsCalculatedExactly()
        {
            var result = Run(100.50m, 3, true);

            Assert.Equal(316.575m, GetTotal(result));
        }
    }
}