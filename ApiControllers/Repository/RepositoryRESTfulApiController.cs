using System;
using System.Data;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Extensions;
using Common.Filters;
using Common.Repositories.Contract;

namespace Common.MVC.ApiControllers.Repository
{
    /// <summary>
    /// Базовый API контроллер редактирования.
    /// Полная поддержка RESTful
    /// </summary> 
    public class RepositoryRESTfulApiController<TEntity, TFilter, TReadRepository, TEditRepository> :
        RepositoryReadApiController<TEntity, TFilter, TReadRepository>
        where TEntity : class
        where TFilter : BaseFilter
        where TReadRepository : IReadRepository<TEntity, TFilter>
        where TEditRepository : IEditRepository<TEntity>
    {
        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        protected const char KeySplitter = '-';

        private readonly Lazy<TEditRepository> _edit;
        private readonly Lazy<TReadRepository> _read;

        public RepositoryRESTfulApiController(Lazy<TReadRepository> read, Lazy<TEditRepository> edit) : base(read)
        {
            _read = read;
            _edit = edit;
        }

        protected TReadRepository read
        {
            get { return _read.Value; }
        }

        protected TEditRepository edit
        {
            get { return _edit.Value; }
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
                edit.Add(entity);
                edit.SaveChanges();
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
            var exist = await read.ExistAsync(key);
            if (!exist) return NotFound();

            try
            {
                await Task.Run(() =>
                {
                    edit.Update(entity);
                    edit.SaveChanges();
                });
            }
            catch (DBConcurrencyException) //DbUpdateConcurrencyException
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
            var exist = await read.ExistAsync(key);
            if (!exist) return NotFound();

            await Task.Run(() =>
            {
                edit.Remove(key);
                edit.SaveChanges();
            });

            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}