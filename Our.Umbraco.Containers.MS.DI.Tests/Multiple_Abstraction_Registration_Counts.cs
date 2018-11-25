using System;
using System.Linq;
using NUnit.Framework;
using Our.Umbraco.Containers.Tests.Registration;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.MS.DI.Tests
{
    [TestFixture]
    public class Multiple_Abstraction_Registration_Counts
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            container = ContainerFactory.Create();
        }

        private void VerifyRegisteredCount(int expected)
        {
            Assert.That(container.GetRegistered<IAbstraction>().ToArray(), Has.Length.EqualTo(expected));
        }

        [Test]
        public void Without_Registering_Service_Leaves_Zero()
        {
            container.Register(typeof(Concrete));
            container.Register(typeof(AnotherConcrete));
            VerifyRegisteredCount(0);
        }

        private void RegisterServiceAndUnnamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), lifetime);
        }

        [Test]
        public void Transient_Leaves_All()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Transient);
            VerifyRegisteredCount(2);
        }

        [Test]
        public void PerRequest_Leaves_One()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Request);
            VerifyRegisteredCount(1);
        }

        [Test]
        public void Scoped_Leaves_One()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Scope);
            VerifyRegisteredCount(1);
        }

        [Test]
        public void Singleton_Leaves_One()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Singleton);
            VerifyRegisteredCount(1);
        }

        [Test]
        public void Instances_Leaves_One()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete());
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            VerifyRegisteredCount(1);
        }

        [Test]
        public void Factories_Transient_Leaves_One()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()));
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()));
            VerifyRegisteredCount(1);
        }
    }
}
