using Common.DTO.Communication;
using Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillWars.Controllers
{
    [Route("api/[controller]")]
    public class ImageStorageController : Controller
    {
        private readonly IImageStorageManager _imageStorageManager;

        public ImageStorageController(IImageStorageManager imageStorageManager)
        {
            _imageStorageManager = imageStorageManager;
        }

        /// <summary>
        /// Upload image
        /// </summary>        
        /// <returns></returns>
        [HttpPost]
        [SwaggerOperation("uploadImage")]
        [ProducesResponseType(typeof(Response<string>), 200)]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            var response = await _imageStorageManager.UploadImageAsync(image.OpenReadStream(), image.FileName);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return Ok(response.Data);
        }

        /// <summary>
        /// Download image by id
        /// </summary>
        /// <param name="id">Image id</param>       
        /// <returns></returns>
        [HttpGet("{id}")]
        [SwaggerOperation("downloadImage")]
        [ProducesResponseType(typeof(Response<FileContentResult>), 200)]
        public async Task<IActionResult> Download([FromRoute] string id)
        {
            var response = await _imageStorageManager.DownloadImageAsync(id);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response.Error);
            }

            return File(response.Data.image, "image/" + response.Data.extension);
        }
    }
}
