/***********************************************************************************************************
 * LINKED FILE!
 * ORIGINAL IN CASTLE TESTS!
 ***********************************************************************************************************/

using System;
using NUnit.Framework;
using Umbraco.Core.Composing;
// ReSharper disable once CheckNamespace
namespace Our.Umbraco.Containers.Tests.Registration
{
    [TestFixture]
    public class Resolving_Abstractions_From_Multiple_Registrations
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            container = ContainerFactory.Create();
        }

        [Test]
        public void Without_Registering_Service_Throws_InvalidOperationException()
        {
            container.Register(typeof(Concrete));
            container.Register(typeof(AnotherConcrete));
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Throws.InstanceOf<InvalidOperationException>());
        }

        private void RegisterServiceAndUnnamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), lifetime);
        }

        [Test]
        public void Transient_Throws_InvalidOperation()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Transient);
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void PerRequest_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Request);
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Scoped_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            { 
                Assert.That(() => container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
            }
        }

        [Test]
        public void Singleton_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Singleton);
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Instances_Resolves_Last_Registered()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete());
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        private void RegisterServiceAndNamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), "Concrete", lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), "AnotherConcrete", lifetime);
        }

        private void VerifyNamedResolves()
        {
            Assert.That(() => container.GetInstance(typeof(IAbstraction), "Concrete"), Is.InstanceOf<Concrete>());
            Assert.That(() => container.GetInstance(typeof(IAbstraction), "AnotherConcrete"), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Named_Transient_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Transient);
            VerifyNamedResolves();
        }

        [Test]
        public void Named_PerRequest_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Request);
            VerifyNamedResolves();
        }

        [Test]
        public void Named_Scoped_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            {
                VerifyNamedResolves();
            }
        }

        [Test]
        public void Named_Type_Singleton_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Singleton);
            VerifyNamedResolves();
        }

        [Test]
        public void Named_Instances_Resolves_Types()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete(), "Concrete");
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete(), "AnotherConcrete");
            VerifyNamedResolves();
        }

        [Test]
        public void Factories_Transient_Resolves_Last_Registered()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()));
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()));
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Named_Factories_Transient_Resolves_Types()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()), "Concrete");
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()), "AnotherConcrete");
            Assert.That(container.GetInstance(typeof(IAbstraction), "Concrete"), Is.InstanceOf<Concrete>());
            Assert.That(container.GetInstance(typeof(IAbstraction), "AnotherConcrete"), Is.InstanceOf<AnotherConcrete>());
        }
    }
}
