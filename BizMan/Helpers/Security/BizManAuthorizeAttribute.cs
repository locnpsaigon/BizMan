using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;

namespace BizMan.Helpers.Security
{
    public class BizManAuthorizeAttribute : AuthorizeAttribute
    {
        protected virtual BizManPrinciple CurrentUser
        {
            get
            {
                return HttpContext.Current.User as BizManPrinciple;
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            bool wasAuthenticated = (CurrentUser != null && CurrentUser.Identity.IsAuthenticated);
            if (!wasAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
            else
            {
                bool wasAuthorized = CurrentUser.IsInRole(Roles) || Users.Contains(CurrentUser.Identity.Name);
                if (!wasAuthorized)
                {
                    filterContext.Result =
                        new RedirectToRouteResult(
                            new RouteValueDictionary(
                                new
                                {
                                    controller = "Error",
                                    action = "AccessDenied",
                                    message = "User have not enough permission!!!"
                                }));
                }
                else
                {
                    // base.OnAuthorization(filterContext);
                    SetCachePolicy(filterContext);
                }
            }
        }

        public void CacheValidationHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }

        protected void SetCachePolicy(AuthorizationContext filterContext)
        {
            // ** IMPORTANT **
            // Since we're performing authorization at the action level, the authorization code runs
            // after the output caching module. In the worst case this could allow an authorized user
            // to cause the page to be cached, then an unauthorized user would later be served the
            // cached page. We work around this by telling proxies not to cache the sensitive page,
            // then we hook our custom authorization code into the caching mechanism so that we have
            // the final say on whether a page should be served from the cache.
            HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
            cachePolicy.SetProxyMaxAge(new TimeSpan(0));
            cachePolicy.AddValidationCallback(CacheValidationHandler, null /* data */);
        }
    }
}