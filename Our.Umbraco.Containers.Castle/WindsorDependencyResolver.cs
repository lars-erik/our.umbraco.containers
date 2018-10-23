using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Castle.Windsor;

namespace Our.Umbraco.Containers.Castle
{
    public class WindsorDependencyResolver : IDependencyResolver
    {
        private readonly WindsorContainer container;

        public WindsorDependencyResolver(WindsorContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return container.ResolveAll(serviceType).Cast<object>();
        }
    }
}