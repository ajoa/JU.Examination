
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace JU.Examination
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            JU.Examination.JUApplExtraction.JU.ADAccess.JUApplInit.ConnectJUApplComponents();

        }

        /// <summary>
        /// Application_s the authenticate request.
        /// </summary>
        protected void Application_AuthenticateRequest()
        {
            // Reading the FormsAuthentication
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || authCookie.Value == "")
            {
                return;
            }

            FormsAuthenticationTicket authTicket;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            // Retrieve roles from UserData 
            string[] roles = authTicket.UserData.Split(';');

            if (Context.User != null)
            {
                Context.User = new System.Security.Principal.GenericPrincipal(Context.User.Identity, roles);
            }
        }
    }
}
