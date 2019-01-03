using System;
using System.Linq;
using NUnit.Framework;
using Our.Umbraco.Containers.Castle.UmbracoTests.Registration;
using Umbraco.Core.Composing;

// ReSharper disable once CheckNamespace
namespace Our.Umbraco.Containers.Tests.Registration
{
    [TestFixture]
    public class Resolving_Default_Abstractions_From_Multiple_Named_Registrations
    {
        private IRegister container;

        [SetUp]
        public void Setup()
        {
            container = RegisterFactory.Create();
        }

        [Test]
        public void Enables_Decoration()
        {
            Assert.Inconclusive();

            //container.Register(typeof(IAbstraction), typeof(Concrete));

            //container.Register(typeof(IAbstraction), container.GetRegistered<IAbstraction>().First().ServiceType, "decorated");
            //// This is the only way to decorate the Concrete registration now
            //container.Register(new Func<IContainer,IAbstraction>(c => new CompositeConcrete(c.GetInstance<IAbstraction>("decorated"))), "decorator");

            //// I want to do
            ////Current.Container.Register(typeof(IAbstraction), typeof(CompositeConcrete), "decorated", new Dictionary<string, string>{{"inner", "decorated"}});

            //// This fails because we don't ask for a named registration...
            //var defaultValue = container.GetInstance(typeof(IAbstraction));
            //Assert.That(
            //    defaultValue, 
            //    Is.InstanceOf<CompositeConcrete>() &
            //    Has.Property("Inner").InstanceOf<Concrete>()
            //);
        }
    }
}
