using System;
using System.Data.Entity.Spatial;
using System.Web.ModelBinding;
using System.Web.Mvc;
using Common.MVC.ModelBinders;
using IMvcModelBinder = System.Web.Mvc.IModelBinder;
using IWebFormsModelBinder = System.Web.ModelBinding.IModelBinder;
using WebFormsModelBindingContext = System.Web.ModelBinding.ModelBindingContext;

namespace Common.MVC.Providers
{
    public class ModelBinderProviderMvc : IModelBinderProvider
    {
        public IMvcModelBinder GetBinder(Type modelType)
        {
            //TODO need IOC
            return modelType == typeof(DbGeography) ? new DbGeographyModelBinder() : null;
        }
    }

    public class ModelBinderProviderWebForms : ModelBinderProvider
    {
        public override IWebFormsModelBinder GetBinder(
            ModelBindingExecutionContext modelBindingExecutionContext,
            WebFormsModelBindingContext bindingContext)
        {
            //TODO need IOC
            return bindingContext.ModelType == typeof(DbGeography) ? new DbGeographyModelBinder() : null;
        }
    }
}