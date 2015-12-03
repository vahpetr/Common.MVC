using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Extensions;
using Common.Filters;
using Common.Services.Contract;

namespace Common.MVC.ApiControllers.Service
{
    /// <summary>
    /// Базовый API контроллер редактирования.
    /// Полная поддержка RESTful
    /// </summary> 
    public class ServiceRESTfulApiController<TEntity, TFilter, TReadService, TEditService> :
        ServiceReadApiController<TEntity, TFilter, TReadService>
        where TEntity : class
        where TFilter : BaseFilter
        where TReadService : IReadService<TEntity, TFilter>
        where TEditService : IEditService<TEntity>
    {
        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        protected const char KeySplitter = '-';

        private readonly Lazy<TEditService> _edit;
        private readonly Lazy<TReadService> _read;
        private readonly Lazy<ITransactionService> _transaction;

        public ServiceRESTfulApiController(Lazy<TReadService> read, Lazy<TEditService> edit,
            Lazy<ITransactionService> transaction) : base(read)
        {
            _read = read;
            _edit = edit;
            _transaction = transaction;
        }

        protected TReadService read
        {
            get { return _read.Value; }
        }

        protected TEditService edit
        {
            get { return _edit.Value; }
        }

        protected ITransactionService transaction
        {
            get { return _transaction.Value; }
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

            transaction.Begin();

            await Task.Run(() =>
            {
                edit.Add(entity);
                edit.Commit();
            });

            transaction.Complete();

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
            var dbEntity = await read.GetAsync(key);
            if (dbEntity == null) return NotFound();

            try
            {
                transaction.Begin();

                await Task.Run(() =>
                {
                    edit.Update(entity, dbEntity);
                    edit.Commit();
                });

                transaction.Complete();
            }
            catch (DBConcurrencyException) //DbUpdateConcurrencyException
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
            var dbEntity = await read.GetAsync(key);
            if (dbEntity == null) return NotFound();

            transaction.Begin();

            await Task.Run(() =>
            {
                edit.Remove(dbEntity);
                edit.Commit();
            });

            transaction.Complete();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}