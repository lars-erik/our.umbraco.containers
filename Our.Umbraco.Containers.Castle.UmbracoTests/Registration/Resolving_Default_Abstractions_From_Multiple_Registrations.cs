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
    public class Resolving_Default_Abstractions_From_Multiple_Registrations
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            container = ContainerFactory.Create();
            container.RegisterInstance(container);
        }

        private void AssertDefaultInstance<TExpected>()
        {
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<TExpected>());
        }

        private void AssertException<TExpectedException>()
            where TExpectedException : Exception
        {
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Throws.InstanceOf<TExpectedException>());
        }

        [Test]
        public void Without_Registering_Service_Throws_InvalidOperationException()
        {
            container.Register(typeof(Concrete));
            container.Register(typeof(AnotherConcrete));
            AssertException<InvalidOperationException>();
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
            AssertException<InvalidOperationException>();
        }

        [Test]
        public void PerRequest_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Request);
            AssertDefaultInstance<AnotherConcrete>();
        }

        [Test]
        public void Scoped_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            {
                AssertDefaultInstance<AnotherConcrete>();
            }
        }

        [Test]
        public void Singleton_Resolves_Last_Registered()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Singleton);
            AssertDefaultInstance<AnotherConcrete>();
        }

        [Test]
        public void Instances_Resolves_Last_Registered()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete());
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            AssertDefaultInstance<AnotherConcrete>();
        }

        private void RegisterServiceAndNamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), "Concrete", lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), "AnotherConcrete", lifetime);
        }

        [Test]
        public void Named_Transient_Throws_InvalidOperationException()
        {
            RegisterServiceAndNamedTypes(Lifetime.Transient);
            AssertException<InvalidOperationException>();
        }

        [Test]
        public void Named_PerRequest_Throws_InvalidOperationException()
        {
            RegisterServiceAndNamedTypes(Lifetime.Request);
            AssertException<InvalidOperationException>();
        }

        [Test]
        public void Named_Scoped_Throws_InvalidOperationException()
        {
            RegisterServiceAndNamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            {
                AssertException<InvalidOperationException>();
            }
        }

        [Test]
        public void Named_Type_Singleton_Throws_InvalidOperationException()
        {
            RegisterServiceAndNamedTypes(Lifetime.Singleton);
            AssertException<InvalidOperationException>();
        }

        [Test]
        public void Named_Instances_Throws_InvalidOperationException()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete(), "Concrete");
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete(), "AnotherConcrete");
            AssertException<InvalidOperationException>();
        }

        [Test]
        public void Factories_Transient_Resolves_Last_Registered()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()));
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()));
            AssertDefaultInstance<AnotherConcrete>();
        }

        [Test]
        public void Named_Factories_Transient_Throws_InvalidOperationException()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()), "Concrete");
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()), "AnotherConcrete");
            AssertException<InvalidOperationException>();
        }
    }
}
