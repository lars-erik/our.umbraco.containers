using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Our.Umbraco.Containers.Tests.Registration;

namespace Our.Umbraco.Containers.Castle.UmbracoTests.Registration
{
    public class CompositeConcrete : IAbstraction
    {
        public IAbstraction Inner { get; }

        public CompositeConcrete(IAbstraction inner)
        {
            this.Inner = inner;
        }
    }
}
