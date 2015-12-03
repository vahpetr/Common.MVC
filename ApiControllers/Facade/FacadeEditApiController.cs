using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Extensions;
using Common.Facades.Contract;
using Common.Filters;

namespace Common.MVC.ApiControllers.Facade
{
    /// <summary>
    /// Базовый API контроллер редактирования.
    /// Полная поддержка RESTful
    /// </summary> 
    public class FacadeEditApiController<TEntity, TFilter, TFacade> : FacadeReadApiController<TEntity, TFilter, TFacade>
        where TEntity : class
        where TFilter : BaseFilter
        where TFacade : IFacade<TEntity, TFilter>
    {
        public FacadeEditApiController(Lazy<TFacade> facade)
            : base(facade)
        {
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

            await facade.Value.Add(entity);

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
            var dbEntity = await facade.Value.Get(key);
            if (dbEntity == null) return NotFound();

            try
            {
                await facade.Value.Update(entity, dbEntity);
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
            var dbEntity = await facade.Value.Get(key);
            if (dbEntity == null) return NotFound();

            await facade.Value.Remove(dbEntity);

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}