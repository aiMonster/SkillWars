using Common.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("NewTransaction")]
        public async Task<IActionResult> NewTransaction()
        {
            await _paymentService.NewTransaction(Request.Form.ToList());
            return Ok();
        }
    }
}
