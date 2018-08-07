using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YxRadio.Gallery.API.Swagger
{
    public class DownloadImageOperation : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.OperationId == "downloadImage")
            {
                operation.Produces = new[] { "image/jpeg", "image/png", "image/jpg" };
                operation.Responses["200"].Schema = new Schema { Type = "file", Description = "Download .png, .jpg or .jpeg image" };
            }
        }
    }
}
