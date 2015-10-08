using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Extensions;
using Common.Filters;
using Common.Repositories.Contract;

namespace Common.MVC.ApiControllers.Service
{
    /// <summary>
    /// Базовый API контроллер редактирования.
    /// Полная поддержка RESTful
    /// </summary> 
#if (!DEBUG)
    [Authorize]
#endif
    public class EditApiController<TEntity, TFilter, TReadRepository, TEditRepository> : ApiController
        where TEntity : class
        where TFilter : BaseFilter
        where TReadRepository : IReadRepository<TEntity, TFilter>
        where TEditRepository : IEditRepository<TEntity>
    {
        private readonly Lazy<TReadRepository> read;
        private readonly Lazy<TEditRepository> edit;

        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        public const char KeySplitter = '-';

        public EditApiController(Lazy<TReadRepository> read, Lazy<TEditRepository> edit)
        {
            this.read = read;
            this.edit = edit;
        }

        /// <summary>
        /// Создать сушность
        /// </summary>
        /// <param name="entity">"Элемент</param>
        /// <returns>Созданная сушность</returns>
        [HttpPost]
        public virtual async Task<IHttpActionResult> Post([FromBody] TEntity entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            await Task.Run(() =>
            {
                edit.Value.Add(entity);
                edit.Value.SaveChanges();
            });

            var key = entity.GetKey();
            var id = string.Join(KeySplitter.ToString(), key);
            return Created(Request.RequestUri + "/" + id, entity);
        }

        /// <summary>
        /// Обновить сушность
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <param name="entity">Элемент</param>
        /// <returns>Обновлённая сушность</returns>
        [HttpPut, Route("{id}")]
        public virtual async Task<IHttpActionResult> Put(string id, TEntity entity)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var key = entity.GetKey();
            if (!Equals(id, string.Join(KeySplitter.ToString(), key))) return BadRequest();

            // ReSharper disable once CoVariantArrayConversion
            var exist = await read.Value.ExistAsync(key);
            if (!exist) return NotFound();

            try
            {
                await Task.Run(() =>
                {
                    edit.Value.Update(entity);
                    edit.Value.SaveChanges();
                });
            }
            catch (DBConcurrencyException)//DbUpdateConcurrencyException
            {
                return Content(HttpStatusCode.Conflict, exist);
            }

            return Ok(entity);
        }

        /// <summary>
        /// Удалить сушность
        /// </summary>
        /// <param name="id">Ключ</param>
        /// <returns>Статус код</returns>
        [HttpDelete, Route("{id}")]
        public virtual async Task<IHttpActionResult> Delete(string id)
        {
            // ReSharper disable once CoVariantArrayConversion
            object[] key = id.Split(KeySplitter);
            var exist = await read.Value.ExistAsync(key);
            if (!exist) return NotFound();

            await Task.Run(() =>
            {
                edit.Value.Remove(key);
                edit.Value.SaveChanges();
            });

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}