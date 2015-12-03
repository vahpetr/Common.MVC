using System;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Filters;
using Common.Repositories.Contract;

namespace Common.MVC.ApiControllers.Repository
{
    /// <summary>
    /// Базовый API контроллер чтения
    /// </summary>
    public class RepositoryReadApiController<TEntity, TFilter, TReadRepository> : ApiController
        where TEntity : class
        where TFilter : BaseFilter
        where TReadRepository : IReadRepository<TEntity, TFilter>
    {
        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        protected const char KeySplitter = '-';

        private readonly Lazy<TReadRepository> _read;

        /// <summary>
        /// Конструктор базового API контроллера
        /// </summary>
        public RepositoryReadApiController(Lazy<TReadRepository> read)
        {
            _read = read;
        }

        protected TReadRepository read
        {
            get { return _read.Value; }
        }

        /// <summary>
        /// Получить страницу сущностей
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <returns>Результат</returns>
        [HttpGet, Route]
        public async Task<IHttpActionResult> Get([FromUri] TFilter filter)
        {
            var result = await read.GetAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Получить сущность по идентификационному номеру
        /// </summary>
        /// <param name="id">Идентификационный номер</param>
        /// <returns>Сущность</returns>
        [HttpGet, Route("{id}")]
        public virtual async Task<IHttpActionResult> Get(string id)
        {
            // ReSharper disable once CoVariantArrayConversion
            object[] key = id.Split(KeySplitter);
            var dbEntity = await read.GetAsync(key);
            if (dbEntity == null) return NotFound();
            return Ok(dbEntity);
        }

        /// <summary>
        /// Проверить существование сущностей
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <returns>Логическое значение</returns>
        [HttpGet, Route("exist")]
        public async Task<IHttpActionResult> Exist([FromUri] TFilter filter)
        {
            var exist = await read.ExistAsync(filter);
            return Ok(exist);
        }

        /// <summary>
        /// Получить количество сущностей
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <returns>Количество сущностей</returns>
        [HttpGet, Route("count")]
        public async Task<IHttpActionResult> Count([FromUri] TFilter filter)
        {
            var count = await read.CountAsync(filter);
            return Ok(count);
        }
    }
}