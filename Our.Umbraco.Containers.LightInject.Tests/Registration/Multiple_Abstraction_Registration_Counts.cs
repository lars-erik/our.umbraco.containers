using System;
using System.Linq;
using NUnit.Framework;
using Our.Umbraco.Containers.Tests.Registration;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.LightInject.Tests.Registration
{
    [TestFixture]
    public class Multiple_Abstraction_Registration_Counts
    {
        private IRegister register;

        [SetUp]
        public void Setup()
        {
            register = RegisterFactory.Create();
        }

        private void VerifyRegisteredCount(int expected)
        {
            Assert.That(register.CreateFactory().GetAllInstances<IAbstraction>().ToArray(), Has.Length.EqualTo(expected));
        }

        [Test]
        public void Without_Registering_Service_Leaves_Zero()
        {
            register.Register(typeof(Concrete));
            register.Register(typeof(AnotherConcrete));
            VerifyRegisteredCount(0);
        }

        private void RegisterServiceAndUnnamedTypes(Lifetime lifetime)
        {
            register.Register(typeof(IAbstraction), typeof(Concrete), lifetime);
            register.Register(typeof(IAbstraction), typeof(AnotherConcrete), lifetime);
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
            register.RegisterInstance(typeof(IAbstraction), new Concrete());
            register.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            VerifyRegisteredCount(1);
        }

        [Test]
        public void Factories_Transient_Leaves_One()
        {
            register.Register((Func<IFactory, IAbstraction>)(c => new Concrete()));
            register.Register((Func<IFactory, IAbstraction>)(c => new AnotherConcrete()));
            VerifyRegisteredCount(1);
        }
    }
}
