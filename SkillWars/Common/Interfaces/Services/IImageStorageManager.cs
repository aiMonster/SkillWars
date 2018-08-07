using Common.DTO;
using Common.DTO.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Services
{
    public interface IImageStorageManager
    {
        Task<Response<string>> UploadImageAsync(Stream image, string name);
        Task<Response<DownloadedFileDTO>> DownloadImageAsync(string id);
    }
}
