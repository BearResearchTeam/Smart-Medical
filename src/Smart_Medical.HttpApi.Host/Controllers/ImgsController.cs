using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.IO;
using Smart_Medical.Until;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Smart_Medical.Controllers
{
    /// <summary>
    /// 图片上传
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "图片上传")]
    public class ImgsController : ControllerBase
    {
        private readonly IWebHostEnvironment webHost;
        private readonly IConfiguration config;

        public ImgsController(IWebHostEnvironment webHost, IConfiguration config)
        {
            this.webHost = webHost;
            this.config = config;
        }
        /// <summary>
        /// 上传图片文件，支持大小和格式校验，保存到指定目录
        /// </summary>
        /// <param name="file">上传的文件</param>
        /// <returns>上传结果，包含文件保存路径</returns>
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // 计算文件大小（单位：MB，取整）
            var fileSize = file.Length / 1024 / 1024;

            // 检查文件大小是否超过配置限制
            if (fileSize > Convert.ToInt64(config["UploadConfig:Limit"]))
            {
                throw new Exception("超出文件大小");
            }

            // 获取文件扩展名（小写）
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            // 校验文件格式是否允许（如果扩展名在禁止列表中则抛出异常）
            if (config["UploadConfig:ExtName"].Split(",").Any(m => m == fileExtension.Trim('.')))
            {
                throw new Exception("上传格式不正确");
            }

            // 创建以日期为名的图片保存目录
            Directory.CreateDirectory($"{webHost.WebRootPath}/Upfile/imgs/{DateTime.Now:yyyyMMdd}");

            // 生成唯一的新文件名
            var newfileName = $"{DateTime.Now:yyyyMMddHHmmssfff}-{Guid.NewGuid()}{fileExtension}";

            // 拼接完整保存路径
            var filePath = $"{webHost.WebRootPath}/Upfile/imgs/{DateTime.Now:yyyyMMdd}/" + newfileName;

            // 保存文件到指定路径
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fs);
                fs.Flush();
            }

            // 返回上传结果
            return Ok($"/Upfile/imgs/{DateTime.Now:yyyyMMdd}/{newfileName}");
           
        }
    }
}
