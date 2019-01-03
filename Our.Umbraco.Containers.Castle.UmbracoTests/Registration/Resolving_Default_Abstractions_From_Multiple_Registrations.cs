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
        private IRegister register;
        private IFactory container;

        [SetUp]
        public void Setup()
        {
            register = RegisterFactory.Create();
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
            register.Register(typeof(Concrete));
            register.Register(typeof(AnotherConcrete));
            AssertException<InvalidOperationException>();
        }

        private void RegisterServiceAndUnnamedTypes(Lifetime lifetime)
        {
            register.Register(typeof(IAbstraction), typeof(Concrete), lifetime);
            register.Register(typeof(IAbstraction), typeof(AnotherConcrete), lifetime);
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
            register.RegisterInstance(typeof(IAbstraction), new Concrete());
            register.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            AssertDefaultInstance<AnotherConcrete>();
        }

        [Test]
        public void Factories_Transient_Resolves_Last_Registered()
        {
            register.Register((Func<IFactory, IAbstraction>)(c => new Concrete()));
            register.Register((Func<IFactory, IAbstraction>)(c => new AnotherConcrete()));
            AssertDefaultInstance<AnotherConcrete>();
        }
    }
}
