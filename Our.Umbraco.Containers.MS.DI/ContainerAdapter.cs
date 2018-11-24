using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.MS.DI
{
    public class ContainerAdapter : IContainer
    {
        private readonly IServiceCollection services;
        private ServiceProvider container;

        public ContainerAdapter()
        {
            services = new ServiceCollection();
        }

        public void Dispose()
        {
            
        }

        private ServiceProvider ServiceProvider
        {
            get
            {
                if (container == null)
                {
                    container = services.BuildServiceProvider();
                }

                return container;
            }
        }

        public object GetInstance(Type type)
        {
            return ServiceProvider.GetService(type);
        }

        public object TryGetInstance(Type type)
        {
            return ServiceProvider.GetService(type);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return ServiceProvider.GetServices(typeof(TService)).Cast<TService>();
        }

        public IEnumerable<Registration> GetRegistered(Type serviceType)
        {
            return services.Select(x => new Registration(x.ServiceType, x.ImplementationType.Name));
        }

        public void Release(object instance)
        {
            // TODO: What does MS.DI require?
        }

        Dictionary<Lifetime, ServiceLifetime> lifetimes = new Dictionary<Lifetime, ServiceLifetime>
        {
            { Lifetime.Request, ServiceLifetime.Scoped },
            { Lifetime.Singleton, ServiceLifetime.Singleton },
            { Lifetime.Scope, ServiceLifetime.Scoped },
            { Lifetime.Transient, ServiceLifetime.Transient }
        };


        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(serviceType, serviceType, lifetimes[lifetime]));
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(serviceType, implementingType, lifetimes[lifetime]));
        }

        public void Register<TService>(Func<IContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            services.Add(new ServiceDescriptor(typeof(TService), sp => factory(this), lifetimes[lifetime]));
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            services.Add(new ServiceDescriptor(serviceType, instance));
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope()
        {
            return ServiceProvider.CreateScope();
        }

        public IContainer ConfigureForWeb()
        {
            // TODO: Figure any dependency
            return this;
        }

        public IContainer EnablePerWebRequestScope()
        {
            // TODO: Figure any dependency;
            return this;
        }

        public object ConcreteContainer => ServiceProvider;
    }
}
