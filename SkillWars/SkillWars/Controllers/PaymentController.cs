using Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger _logger;

        public PaymentController(IPaymentService paymentService, ILoggerFactory loggerFactory)
        {
            _paymentService = paymentService;
            _logger = loggerFactory.CreateLogger<PaymentController>();
        }

        [HttpPost("NewTransaction")]
        public async Task<IActionResult> NewTransaction()
        {

            //StreamReader reader = new StreamReader(Request.Body);
            //string text = reader.ReadToEnd();

            
            //_logger.LogError(text);
            //await _paymentService.NewTransaction(Request.Form.ToList());
            return Ok();
        }
    }
}
