using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.Optimization;
using Newtonsoft.Json;
using BizMan.Helpers.Security;

namespace BizMan
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // prevent code first model initializing database for the first time
            System.Data.Entity.Database.SetInitializer<DAL.DataContext>(null);

            // online user counter
            Application["OnlineVisitors"] = 0;
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Application.Lock();
            // increate online user counter
            Application["OnlineVisitors"] = (int)Application["OnlineVisitors"] + 1;
            Application.UnLock();
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Application.Lock();
            // decreate online user counter
            Application["OnlineVisitors"] = (int)Application["OnlineVisitors"] - 1;
            Application.UnLock();
        }

        protected void Application_PostAuthenticateRequest()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                
                BizManPrincipalSerialize model = JsonConvert.DeserializeObject<BizManPrincipalSerialize>(authTicket.UserData);
                BizManPrinciple userLogin = new BizManPrinciple(authTicket.Name);
                userLogin.UserId = model.UserId;
                userLogin.FirstName = model.FirstName;
                userLogin.LastName = model.LastName;
                userLogin.CreationDate = model.CreationDate;
                userLogin.Roles = model.Roles;
                HttpContext.Current.User = userLogin;
            }
        }

    }
}