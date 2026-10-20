using Calculator.Data;
using Calculator.Models;
using Calculator.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Calculator.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly CalculatorContext _context;
        private readonly KafkaProducerService<Null, string> _producer;

        public CalculatorController(
            CalculatorContext context,
            KafkaProducerService<Null, string> producer)
        {
            _context = context;
            _producer = producer;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var last = _context.DataInputVariants
                .OrderByDescending(x => x.ID_DataInputVariant)
                .FirstOrDefault();
            ViewBag.LastResult = last?.Result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(double num1, double num2, Operation operation)
        {
            var dataInputVariant = new DataInputVariant
            {
                Operand_1 = num1,
                Operand_2 = num2,
                Type_operation = operation
            };

            var json = JsonSerializer.Serialize(dataInputVariant);
            await _producer.ProduceAsync("materenko", new Message<Null, string> { Value = json });

           
            await Task.Delay(1000);

            
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        public IActionResult Callback([FromBody] DataInputVariant inputData)
        {
            if (inputData != null)
            {
                _context.DataInputVariants.Add(inputData);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}