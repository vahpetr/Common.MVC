using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Extensions;
using Common.Services.Contract;

namespace Common.MVC.ApiControllers.Repository
{
    /// <summary>
    /// Базовый API контроллер редактирования.
    /// Полная поддержка RESTful
    /// </summary> 
#if (!DEBUG)
    [Authorize]
#endif
    public class EditApiController<TEntity, TFilter, TReadService, TEditService> : ApiController
        where TEntity : class
        where TFilter : class
        where TReadService : IReadService<TEntity, TFilter>
        where TEditService : IEditService<TEntity>
    {
        private readonly Lazy<TReadService> read;
        protected readonly Lazy<TEditService> edit;
        private readonly Lazy<ITransactionService> transaction;

        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        public const char KeySplitter = '-';

        public EditApiController(Lazy<TReadService> read, Lazy<TEditService> edit, Lazy<ITransactionService> transaction)
        {
            this.read = read;
            this.edit = edit;
            this.transaction = transaction;
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

            transaction.Value.Begin();

            await Task.Run(() =>
            {
                edit.Value.Add(entity);
                edit.Value.Commit();
            });

            transaction.Value.Complete();

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
            var dbEntity = await read.Value.GetAsync(key);
            if (dbEntity == null) return NotFound();

            try
            {
                transaction.Value.Begin();

                await Task.Run(() =>
                {
                    edit.Value.Update(entity, dbEntity);
                    edit.Value.Commit();
                });

                transaction.Value.Complete();
            }
            catch (DBConcurrencyException)//DbUpdateConcurrencyException
            {
                return Content(HttpStatusCode.Conflict, dbEntity);
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
            var dbEntity = await read.Value.GetAsync(key);
            if (dbEntity == null) return NotFound();

            transaction.Value.Begin();

            await Task.Run(() =>
            {
                edit.Value.Remove(dbEntity);
                edit.Value.Commit();
            });

            transaction.Value.Complete();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}