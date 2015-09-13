using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace Common.MVC.Providers
{
    //public class KeyConstraint : IRouteConstraint
    //{
    //    public bool Match(HttpContextBase context, Route route, string parameterName, RouteValueDictionary values,
    //        RouteDirection direction)
    //    {
    //        return !string.IsNullOrEmpty(parameterName);
    //    }
    //}

    public class InheritDirectRouteProvider : DefaultDirectRouteProvider
    {
        protected override IReadOnlyList<IDirectRouteFactory>
        GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(true);
        }
    }
}
