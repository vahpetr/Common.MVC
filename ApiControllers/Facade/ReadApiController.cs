using System;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Facades.Contract;
using Common.Filters;

namespace Common.MVC.ApiControllers.Facade
{
    /// <summary>
    /// Базовый API контроллер чтения
    /// </summary>
#if (!DEBUG)
    [Authorize]
#endif
    public class ReadApiController<TEntity, TFilter, TFacade> : ApiController
        where TEntity : class
        where TFilter : BaseFilter
        where TFacade : IFacade<TEntity, TFilter>
    {
        protected readonly Lazy<TFacade> facade;

        /// <summary>
        /// Разделитель составного первичного ключа
        /// </summary>
        public const char KeySplitter = '-';

        /// <summary>
        /// Конструктор базового API контроллера
        /// </summary>
        public ReadApiController(Lazy<TFacade> facade)
        {
            this.facade = facade;
        }

        /// <summary>
        /// Получить страницу сущностей
        /// </summary>
        /// <param name="filter">Фильтр</param>
        /// <returns>Результат</returns>
        [HttpGet, Route]
        public async Task<IHttpActionResult> Get([FromUri] TFilter filter)
        {
            var result = await facade.Value.Get(filter);
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
            var dbEntity = await facade.Value.Get(key);
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
            var exist = await facade.Value.Exist(filter);
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
            var count = await facade.Value.Count(filter);
            return Ok(count);
        }
    }
}