using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Umbraco.Core.Composing;
using Umbraco.Core.Exceptions;

namespace Our.Umbraco.Containers.MS.DI
{
    public class ConcreteMsDi
    {
        public IServiceCollection ServiceCollection { get; set; }
        public IServiceProvider Provider { get; set; }

        public ConcreteMsDi(IServiceCollection serviceCollection, IServiceProvider provider)
        {
            ServiceCollection = serviceCollection;
            Provider = provider;
        }
    }

    public class ContainerAdapter : IRegister, IFactory, IDisposable
    {
        private readonly IServiceCollection services;
        private readonly Stack<ScopeWrapper> scopes = new Stack<ScopeWrapper>();

        public static IRegister Create()
        {
            var msDiContainer = new ContainerAdapter();
            msDiContainer.RegisterInstance<IFactory>(msDiContainer);
            return msDiContainer;
        }

        public IFactory CreateFactory()
        {
            return this;
        }

        private ConcreteMsDi concrete;

        public object Concrete => concrete;

        private ServiceProvider container;

        public ContainerAdapter()
        {
            services = new ServiceCollection();

            services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        }

        internal class Lazier<T> : Lazy<T> where T : class
        {
            public Lazier(IServiceProvider provider)
                : base(() => ResolveService(provider))
            {
            }

            private static T ResolveService(IServiceProvider provider)
            {
                try
                {
                    var requiredService = provider.GetRequiredService<T>();
                    return requiredService;
                }
                catch(Exception ex)
                {
                    throw new Exception("Failed to lazy initialize", ex);
                }
            }
        }

        public void Dispose()
        {
            
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                if (container == null)
                {
                    var all = ServicesTable();

                    container = services.BuildServiceProvider();
                    concrete = new ConcreteMsDi(services, container);
                }

                if (scopes.Any())
                {
                    return scopes.Peek();
                }

                return container;
            }
        }

        private string ServicesTable()
        {
            var serviceData = ServicesLines();
            var all = String.Join(Environment.NewLine, serviceData);
            return all;
        }

        private IEnumerable<string> ServicesLines()
        {
            var serviceData = services.Select(x =>
                $"{x.ServiceType.Name}; {x.ImplementationType?.Name}; {x.ImplementationInstance?.GetType().Name}; {x.Lifetime}"
            );
            return serviceData;
        }

        private void Reset()
        {
            container = null;
        }

        public object CreateWithParameters(Type type, object[] args)
        {
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            if (ctor == null)
                throw new InvalidOperationException($"Could not find a public constructor for type {type.FullName}.");

            var ctorParameters = ctor.GetParameters();
            var ctorArgs = new object[ctorParameters.Length];
            var i = 0;
            foreach (var parameter in ctorParameters)
            {
                // no! IsInstanceOfType is not ok here
                // ReSharper disable once UseMethodIsInstanceOfType
                var arg = args?.FirstOrDefault(a => parameter.ParameterType.IsAssignableFrom(a.GetType()));
                ctorArgs[i++] = arg ?? GetInstance(parameter.ParameterType);
            }

            return ctor.Invoke(ctorArgs);
        }

        public object GetInstance(Type type)
        {
            return ServiceProvider.GetService(type);
        }

        public object TryGetInstance(Type type)
        {
            try
            {
                return ServiceProvider.GetService(type);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return ServiceProvider.GetServices(serviceType);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return ServiceProvider.GetServices(typeof(TService)).Cast<TService>();
        }

        //public IEnumerable<Registration> GetRegistered(Type serviceType)
        //{
        //    return services
        //        .Where(x => serviceType.IsAssignableFrom(x.ServiceType))
        //        .Select(x => new Registration(x.ImplementationType ?? x.ServiceType, x.ImplementationType?.Name ?? x.ServiceType?.Name));
        //}

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
            Reset();
            services.Add(new ServiceDescriptor(serviceType, serviceType, lifetimes[lifetime]));
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            Reset();
            services.Add(new ServiceDescriptor(serviceType, implementingType, lifetimes[lifetime]));
        }

        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
        {
            Reset();
            services.Add(new ServiceDescriptor(typeof(TService), sp => factory(this), lifetimes[lifetime]));
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            Reset();
            services.Add(new ServiceDescriptor(serviceType, instance));
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            Reset();
            try
            {
                // TODO: Figure out if fallback registration is allowed. IE. What does MS. do with runtime generated views?
                
                //services.Scan(scan =>
                //    scan.FromApplicationDependencies(x => x != null)
                //        .AddClasses(x => x.AssignableTo(serviceBaseType))
                //        .AsImplementedInterfaces()
                //);
            }
            catch (Exception ex)
            {
                throw new BootFailedException("Autoregister failed", ex);
            }
        }

        public IDisposable BeginScope()
        {
            var scope = ServiceProvider.CreateScope();
            var wrapper = new ScopeWrapper(scope, this);
            scopes.Push(wrapper);
            return wrapper;
        }

        public void ConfigureForWeb()
        {
            // TODO: Figure any dependency
        }

        public void EnablePerWebRequestScope()
        {
            // TODO: Figure any dependency;
        }

        class ScopeWrapper : IServiceProvider, IDisposable
        {
            private IServiceScope scope;
            private readonly ContainerAdapter adapter;
            private IServiceProvider provider;

            public ScopeWrapper(IServiceScope scope, ContainerAdapter adapter)
            {
                this.scope = scope;
                this.adapter = adapter;
                this.provider = scope.ServiceProvider;
            }

            public void Dispose()
            {
                scope.Dispose();
                adapter.scopes.Pop();
            }

            public object GetService(Type serviceType)
            {
                return provider.GetService(serviceType);
            }
        }
    }
}
