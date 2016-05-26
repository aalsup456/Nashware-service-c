using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Configuration;
using System.Web.Http.Cors;

namespace TNG.NashWare.Services
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //var clientID = WebConfigurationManager.AppSettings["auth0:ClientId"];
            //var clientSecret = WebConfigurationManager.AppSettings["auth0:ClientSecret"];

            //config.MessageHandlers.Add(new JsonWebTokenValidationHandler()
            //{
            //    Audience = clientID,
            //    SymmetricKey = clientSecret
            //});

            var corsAttr = new EnableCorsAttribute(origins: "*", headers: "accept, content-type, origin, *", methods: "*")
            {
                //SupportsCredentials = true
            };
            config.EnableCors(corsAttr);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
