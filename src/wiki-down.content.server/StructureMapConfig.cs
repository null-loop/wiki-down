using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using StructureMap;
using StructureMap.Building;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using wiki_down.core;
using wiki_down.core.storage;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

namespace wiki_down.content.server
{
    public class ControllerRegistrationConvention : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            if ((typeof (Controller).IsAssignableFrom(type) || typeof (ApiController).IsAssignableFrom(type)) && !type.IsAbstract)
            {
                registry.For(type).Use(type);    
            }
            
        }
    }

    public static class StructureMapConfig
    {
        private static Container _container;

        public static void Configure()
        {
            _container = new Container(ce =>
            {
                ce.Scan(y =>
                {
                    y.AssembliesFromApplicationBaseDirectory();
                    y.WithDefaultConventions();
                    y.With(new ControllerRegistrationConvention());
                    y.AddAllTypesOf<Controller>();
                    y.AddAllTypesOf<ApiController>();
                });

                ce.For<ISystemAuditService>().Use(() => MongoDataStore.SystemAuditStore);
                ce.For<ISystemLoggingService>().Use(() => MongoDataStore.SystemLoggingStore);
                ce.For<ISystemConfigurationService>().Use(() => MongoDataStore.SystemConfigurationStore);

                ce.For<IArticleService>().Use(() => MongoDataStore.CreateStore<MongoArticleStore>());
                ce.For<IJavascriptFunctionService>().Use(() => MongoDataStore.CreateStore<MongoJavascriptFunctionStore>());
                ce.For<IGeneratedArticleContentService>().Use(() => MongoDataStore.CreateStore<MongoGeneratedArticleContentStore>());
                ce.For<IArticleMetaDataService>().Use(() => MongoDataStore.CreateStore<MongoArticleMetaDataStore>());
            });
        }

        public static void ConfigureForMvc()
        {
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(Container, DependencyResolver.Current));
        }

        public static void ConfigureForWebApi(HttpConfiguration config)
        {
            config.DependencyResolver = new StructureMapDependencyResolver(Container, DependencyResolver.Current);
            config.Services.Replace(typeof(IHttpControllerActivator), new StructureMapActivator(config));
        }

        public static Container Container { get {  return _container;} }
    }

    public class StructureMapActivator : IHttpControllerActivator
    {
        public StructureMapActivator(HttpConfiguration configuration) { }

        public IHttpController Create(HttpRequestMessage request
            , HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return DependencyResolver.Current.GetService(controllerType) as IHttpController;
        }
    }

    public class StructureMapDependencyResolver : IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly IContainer _container;
        private readonly IDependencyResolver _resolver;

        public StructureMapDependencyResolver(IContainer container, IDependencyResolver resolver)
        {
            _container = container;
            _resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            return _container.TryGetInstance(serviceType) ?? _resolver.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _container.GetAllInstances(serviceType).OfType<object>();
            }
            catch (Exception)
            {
                return _resolver.GetServices(serviceType);
            }
        }

        public IDependencyScope BeginScope()
        {
            var child = _container.CreateChildContainer();
            return new StructureMapDependencyResolver(child, _resolver);
        }

        public void Dispose()
        {
        }
    }
}
