﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using wiki_down.core.storage;

namespace wiki_down.content.server
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

            StructureMapConfig.Configure();
            TemplatesConfig.Configure();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(config =>
            {
                WebApiConfig.Register(config);
                StructureMapConfig.ConfigureForWebApi(config);
            });
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            StructureMapConfig.ConfigureForMvc();
            MongoDataStore.Initialise(ConfigurationManager.AppSettings["wikidown.mongodb.connectionString"],
                ConfigurationManager.AppSettings["wikidown.mongodb.dbName"]);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}