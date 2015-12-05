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
        private readonly Lazy<TEditService> _edit;
        private readonly Lazy<ITransactionService> _transaction;

        public ServiceRESTfulApiController(Lazy<TReadService> read, Lazy<TEditService> edit,
            Lazy<ITransactionService> transaction) : base(read)
        {
            _edit = edit;
            _transaction = transaction;
        }

        protected TEditService edit => _edit.Value;

        protected ITransactionService transaction => _transaction.Value;

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

            //TODO пока поправил падение так, лучше нечего не придумал
            edit.Add(entity);
            await edit.CommitAsync();

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
                
                //TODO пока поправил падение так, лучше нечего не придумал
                edit.Update(entity, dbEntity);
                await edit.CommitAsync();

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

            //TODO пока поправил падение так, лучше нечего не придумал
            edit.Remove(dbEntity);
            await edit.CommitAsync();

            transaction.Complete();

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}