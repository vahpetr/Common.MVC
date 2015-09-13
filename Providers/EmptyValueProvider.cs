using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace Common.MVC.Providers
{
    public class EmptyValueProvider : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return prefix != null && prefix.Length == 0;
        }

        public ValueProviderResult GetValue(string key)
        {
            return null;
        }
    }
    public class EmptyValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        private static readonly EmptyValueProvider ValueProvider = new EmptyValueProvider();

        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return actionContext.ControllerContext.RouteData.Values.Count == 0 ? ValueProvider : null;
        }
    }
}
