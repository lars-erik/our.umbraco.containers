using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.MetadataServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Diagnostics.Helpers;
using Castle.Windsor.Installer;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.Castle
{
    public class Registration
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public Registration(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class CastleContainer : IRegister, IFactory, IDisposable
    {
        // fixme - creates a cyclic dependency with castle
        public static IRegister Create()
        {
            var castleContainer = new CastleContainer();
            //castleContainer.RegisterInstance<I>(castleContainer);
            return castleContainer;
        }

        private Dictionary<Lifetime, LifestyleType> lifetimes = new Dictionary<Lifetime, LifestyleType>
        {
            { Lifetime.Request, LifestyleType.PerWebRequest },
            { Lifetime.Scope, LifestyleType.Scoped },
            { Lifetime.Singleton, LifestyleType.Singleton },
            { Lifetime.Transient, LifestyleType.Transient },
        };

        private WindsorContainer container;

        public object CreateWithParameters(Type type, object[] args)
        {
            throw new NotImplementedException();
        }

        public object Concrete => container;

        public CastleContainer()
        {
            container = new WindsorContainer();
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));
        }

        public void Dispose()
        {
            container.Dispose();
        }

        public IFactory CreateFactory()
        {
            return this;
        }

        public object GetInstance(Type type)
        {
            try
            {
                return container.Resolve(type);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new InvalidOperationException("Could not find default instance of " + type.Name, ex);
            }
        }

        public object GetInstance(Type type, string name)
        {
            try
            {
                return container.Resolve(name, type);
            }
            catch (ComponentNotFoundException ex)
            {
                throw new InvalidOperationException("Could not find instance of " + type.Name + " named " + name, ex);
            }
        }

        // fixme - figure out what castle actually does vs. xml doc
        public object TryGetInstance(Type type)
        {
            try
            {
                return container.Resolve(type);
            }
            catch(Exception ex)
            {
                try
                {
                    if (typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        if (type.GenericTypeArguments.Length == 1)
                        {
                            var result = GetAllInstances(type.GenericTypeArguments[0]);
                            if (result != null && result.Any())
                            {
                                return result;
                            }
                        }
                    }
                }
                catch (Exception ex2)
                {
                    throw new InvalidOperationException("Could not find default instance of " + type.Name, ex);
                }

                throw new InvalidOperationException("Could not find instance of " + type.Name);
            }
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.ResolveAll(serviceType).Cast<object>();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return container.ResolveAll<TService>();
        }

        // fixme - Translate to Registration object
        public IEnumerable<Registration> GetRegistered(Type serviceType)
        {
            return container.Kernel.GetAssignableHandlers(serviceType)
                .Select(x => new Registration(serviceType, x.ComponentModel.Name)
            );
        }

        public IEnumerable<Registration> GetRegistered()
        {
            return container.Kernel.GetAssignableHandlers(typeof(object))
                .Select(x => new Registration(x.ComponentModel.Services.First(), x.ComponentModel.Name));

        }

        private ComponentRegistration<object> NewDefault(Type serviceType, Lifetime lifetime, string name = null)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (container.Kernel.HasComponent(name))
                {
                    container.RemoveHandler(name);
                }
            }

            var registration = Component
                .For(serviceType)
                .IsDefault()
                .LifeStyle.Is(lifetimes[lifetime]);
            if (name != null)
                registration = registration.Named(name);
            return registration;
        }

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(NewDefault(serviceType, lifetime));
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                NewDefault(serviceType, lifetime)
                    .ImplementedBy(implementingType)
            );
        }

        public void Register(Type serviceType, Type implementingType, string name, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                NewDefault(serviceType, lifetime, name)
                    .ImplementedBy(implementingType)
            );
        }

        // fixme - Must make TService reference type in interface
        // fixme - Umbraco re-registers IProfiler for instance. Must set new registrations as default. (?)
        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                NewDefault(
                    typeof(TService),
                    lifetime,
                    typeof(TService).Name + "-impl-" + Guid.NewGuid().ToString("N")
                )
                .UsingFactory<IFactory, TService>(f => factory(this))
            );

        }

        // fixme - Must make TService reference type in interface
        // fixme - Umbraco re-registers IProfiler for instance. Must set new registrations as default. (?)
        public void Register<TService>(Func<IFactory, TService> factory, string name, Lifetime lifetime = Lifetime.Transient)
        {
            container.Register(
                NewDefault(
                    typeof(TService),
                    lifetime,
                    name
                )
                .UsingFactory<IFactory, TService>(f => factory(this))
            );

        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            container.Register(
                NewDefault(serviceType, Lifetime.Singleton)
                    .Instance(instance)
            );
        }

        public void RegisterInstance(Type serviceType, object instance, string name)
        {
            container.Register(
                NewDefault(serviceType, Lifetime.Singleton, name)
                    .Instance(instance)
            );
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            container.Register(
                Classes
                .FromAssemblyContaining(serviceBaseType)
                .BasedOn(serviceBaseType)
            );
        }

        // fixme Figure out if Castle returns things in same order as registered
        public void RegisterOrdered(Type serviceType, Type[] implementingTypes, Lifetime lifetime = Lifetime.Transient)
        {
            foreach (var type in implementingTypes)
            {
                container.Register(
                    NewDefault(serviceType, lifetime)
                        .ImplementedBy(type)
                );
            }
        }

        public void Release(object instance)
        {
            container.Kernel.ReleaseComponent(instance);
        }

        public IDisposable BeginScope()
        {
            return container.BeginScope();
        }

        // fixme - Figure out requirements
        public void ConfigureForWeb()
        {
            //Register<IFilteredControllerFactory>(x => new WindsorControllerFactory(this), Lifetime.Singleton);

            //GlobalConfiguration.Configuration.DependencyResolver = new WindsorWebApiDependencyResolver(container);

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new WindsorCompositionRoot(this.container));
        }

        // fixme - Figure out if this is on by default and how to set it.
        public void EnablePerWebRequestScope()
        {
        }
    }
}

