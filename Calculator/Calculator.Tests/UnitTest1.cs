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

        // ========== Позитивные тесты ==========

        // Тест 1 - Happy Path
        [Fact]
        public void Calculate_WithoutInsurance_ReturnsOkAndCorrectAmount()
        {
            var result = Run(100, 5, false);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(500m, GetTotal(result));
        }

        // Тест 2 - со страховкой (поиск бага)
        [Fact]
        public void Calculate_WithInsurance_AddsFivePercent()
        {
            var result = Run(100, 5, true);

            Assert.Equal(525m, GetTotal(result));
        }

        // Тест 3 - нулевая стоимость дня
        [Fact]
        public void Calculate_ZeroDailyCost_ReturnsZero()
        {
            var result = Run(0, 5, false);

            Assert.Equal(0m, GetTotal(result));
        }

        // Тест 4 - копейки без страховки
        [Fact]
        public void Calculate_DecimalDailyCost_CalculatesToKopecks()
        {
            var result = Run(100.50m, 3, false);

            Assert.Equal(301.50m, GetTotal(result));
        }

        // Тест 5 - копейки со страховкой
        [Fact]
        public void Calculate_DecimalWithInsurance_IsCalculatedExactly()
        {
            var result = Run(100.50m, 3, true);

            Assert.Equal(316.575m, GetTotal(result));
        }

        // ========== Негативные тесты ==========
        [Theory]
        [InlineData(-1)] // Тест 6
        [InlineData(0)]  // Тест 7
        public void Calculate_NonPositiveDays_ReturnsBadRequest(int days)
        {
            var result = Run(100, days, false);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}