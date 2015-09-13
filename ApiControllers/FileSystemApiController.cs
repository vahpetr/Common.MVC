using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.MVC.HttpActionResults;
using Common.MVC.Providers;

namespace Common.MVC.ApiControllers
{
    /// <summary>
    /// Контроллер работы с файловой системой
    /// </summary>
#if (!DEBUG)
    [Authorize]
#endif
    [RoutePrefix("api/filesystem")]
    public class FileSystemApiController : ApiController
    {
        private static readonly string Temp;

        static FileSystemApiController()
        {
            Temp = ConfigurationManager.AppSettings["temp"] ?? "C:\\Temp";
            if (!Directory.Exists(Temp)) Directory.CreateDirectory(Temp);
        }

        /// <summary>
        /// Сохранить файл
        /// </summary>
        /// <param name="id">Название файла. Не обязательно</param>
        /// <returns>Название файла</returns>
        [HttpPost, Route("{id?}")]
        public async Task<IHttpActionResult> Post(string id)
        {
            if (!Request.Content.IsMimeMultipartContent())
                return StatusCode(HttpStatusCode.UnsupportedMediaType);

            var key = Path.GetFileName(id);
            if (string.IsNullOrEmpty(key)) key = Guid.NewGuid().ToString("N");

            var path = Path.Combine(Temp, key);
            if (File.Exists(path)) File.Delete(path);

            try
            {
                var provider = new DiskMultipartFormDataStreamOneFileProvider(Temp, key);
                await Request.Content.ReadAsMultipartAsync(provider);
            }
            catch (Exception)
            {
                if (File.Exists(path)) File.Delete(path);
                throw;
            }

            return Ok(id);
        }

        /// <summary>
        /// Получить файл
        /// </summary>
        /// <param name="id">Название файла</param>
        /// <returns>Файл</returns>
        [HttpGet, Route("{id}")]
        public IHttpActionResult Get(string id)
        {
            var key = Path.GetFileName(id);
            var path = Path.Combine(Temp, key);
            if (!File.Exists(path)) return NotFound();

            return new ResponseFile(path);
        }

        /// <summary>
        /// Удалить файл
        /// </summary>
        /// <param name="id">Название файла</param>
        /// <returns>Статус код</returns>
        [HttpDelete, Route("{id}")]
        public IHttpActionResult Delete(string id)
        {
            var key = Path.GetFileName(id);
            var path = Path.Combine(Temp, key);
            if (!File.Exists(path)) return NotFound();

            File.Delete(path);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}