using Common.DTO.Communication;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PaymentController(IPaymentService paymentService, IHostingEnvironment hostingEnvironment)
        {
            _paymentService = paymentService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("NewTransaction")]
        public async Task<IActionResult> NewTransaction()
        {            
            await _paymentService.NewTransaction(Request.Form.ToList());
            return Ok();
        }

        /// <summary>
        /// 
        ///</summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post(IFormFile uploadedFile)
        {
            var logPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Multimedia\\" + uploadedFile.FileName);
            
            byte[] data;            
            var br = new BinaryReader(uploadedFile.OpenReadStream());
            data = br.ReadBytes((int)uploadedFile.OpenReadStream().Length);

            using (var fs = new FileStream(logPath, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
            return Ok(uploadedFile.FileName);
        }
        
        [HttpGet("download/{id}")]
        [SwaggerOperation("getById")]
        public async Task<IActionResult> Get ([FromRoute] string id)
        {
            //var stream = System.IO.File.Open(@"C:\Users\Student\Desktop\9ea_borns_image.jpg",FileMode.Open, FileAccess.Read);
            //var response = File(stream, "application/octet-stream"); // FileStreamResult
            //return response;



            //return PhysicalFile(@"C:\Users\Student\Desktop\9ea_borns_image.jpg", "application/octet-stream", "9ea_borns_image.jpg");



            //var memory = new MemoryStream();
            //using (var stream = new FileStream(@"C:\Users\Student\Desktop\steam.txt", FileMode.Open))
            //{
            //    await stream.CopyToAsync(memory);
            //}
            //memory.Position = 0;

            var logPath = Path.Combine(_hostingEnvironment.ContentRootPath, @"Multimedia\" + id);
            //if(!System.IO.File.Exists(logPath))
            //{
            //    return StatusCode(404, new Error(500, "Server is brocken, we need to update api key, write to developers"));
            //}
            var memory = System.IO.File.ReadAllBytes(logPath);
            var extension = logPath.Split('.').Last();
            return File(memory, "image/" + extension);
        }

    }
}
