using System.Data.Entity.Spatial;
using System.Globalization;
using System.Web.ModelBinding;
using System.Web.Mvc;
using IMvcModelBinder = System.Web.Mvc.IModelBinder;
using IWebFormsModelBinder = System.Web.ModelBinding.IModelBinder;
using MvcModelBindingContext = System.Web.Mvc.ModelBindingContext;
using WebFormsModelBindingContext = System.Web.ModelBinding.ModelBindingContext;

namespace Common.MVC.ModelBinders
{
    public class DbGeographyModelBinder : IMvcModelBinder, IWebFormsModelBinder
    {
        public object BindModel(ControllerContext controllerContext, MvcModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            return Bind(valueProviderResult != null ? valueProviderResult.AttemptedValue : null);
        }

        public bool BindModel(ModelBindingExecutionContext modelBindingExecutionContext,
            WebFormsModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.Model = Bind(valueProviderResult != null ? valueProviderResult.AttemptedValue : null);
            return bindingContext.Model != null;
        }

        private DbGeography Bind(string value)
        {
            if (value == null) return default(DbGeography);

            var position = value.Split(',');

            if (position.Length != 2) return default(DbGeography);

            //4326 format puts LONGITUDE first then LATITUDE
            var point = string.Format(CultureInfo.InvariantCulture, "POINT ({0} {1})", position[1], position[0]);
            var result = DbGeography.FromText(point, 4326); // DbGeography.DefaultCoordinateSystemId
            return result;
        }
    }
}