using Common.DTO;
using Common.DTO.Communication;
using Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ImageStorageManager
{
    public class ImageStorageManager : IImageStorageManager
    {
        private readonly string _path;

        public ImageStorageManager(string path)
        {
            _path = path;
        }

        public async Task<Response<string>> UploadImageAsync(Stream image, string name)
        {
            var response = new Response<string>();
            var extensions = new string[] { "png", "jpeg", "jpg" };

            var ext = name.Split('.');
            if (ext.Length == 1 || !extensions.Contains(ext.Last()))
            {
                response.Error = new Error(409, "Not compatible extension");
                return response;
            }
            var id = Guid.NewGuid().ToString();

            var br = new BinaryReader(image);
            var data = br.ReadBytes((int)image.Length);
            using (var fs = new FileStream(_path + "\\" + id + "." + ext.Last(), FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(data, 0, data.Length);
            }
            response.Data = id;
            return response;
        }

        public async Task<Response<DownloadedFileDTO>> DownloadImageAsync(string id)
        {
            var response = new Response<DownloadedFileDTO>();
            var files = Directory.GetFiles(_path, id + ".*");
            if (!files.Any())
            {
                response.Error = new Error(404, "Such image not found");
                return response;
            }

            response.Data = new DownloadedFileDTO()
            {
                image =  File.ReadAllBytes(files[0]),
                extension = files[0].Split('.').Last()
            };
            return response;
        }
    }
}
