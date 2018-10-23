using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.Castle.UmbracoTests.Defaults
{
    [TestFixture]
    public class Defaults
    {
        private IContainer container;

        [SetUp]
        public void Setup()
        {
            container = ContainerFactory.Create();
        }

        [Test]
        public void Multiple_Type_Without_Service_Is_Irrelevant()
        {
            container.Register(typeof(Concrete));
            container.Register(typeof(AnotherConcrete));
            Assert.That(container.GetInstance(typeof(Concrete)), Is.InstanceOf<Concrete>());
        }

        private void RegisterServiceAndUnnamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), lifetime);
        }

        [Test]
        public void Service_And_Multiple_Type_Transient_Throws_InvalidOperation()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Transient);
            Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(2));
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Throws.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void Service_And_Multiple_Type_PerRequest_Removes_Previous_Registration()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Request);
            Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(1));
            Assert.That(() => container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Service_And_Multiple_Type_Scoped_Removes_Previous_Registration()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            { 
                Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(1));
                Assert.That(() => container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
            }
        }

        [Test]
        public void Service_And_Multiple_Type_Singleton_Removes_Previous_Registration()
        {
            RegisterServiceAndUnnamedTypes(Lifetime.Singleton);
            Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(1));
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Service_And_Multiple_Instances_Removes_Previous_Instance()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete());
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete());
            Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(1));
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }

        private void RegisterServiceAndNamedTypes(Lifetime lifetime)
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), "Concrete", lifetime);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), "AnotherConcrete", lifetime);
        }

        private void VerifyNamedResolves()
        {
            Assert.That(container.GetAllInstances(typeof(IAbstraction)), Has.Length.EqualTo(2));
            Assert.That(() => container.GetInstance(typeof(IAbstraction), "Concrete"), Is.InstanceOf<Concrete>());
            Assert.That(() => container.GetInstance(typeof(IAbstraction), "AnotherConcrete"), Is.InstanceOf<AnotherConcrete>());
        }

        [Test]
        public void Service_And_Multiple_Named_Type_Transient_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Transient);
            VerifyNamedResolves();
        }

        [Test]
        public void Service_And_Multiple_Named_Type_PerRequest_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Request);
            VerifyNamedResolves();
        }

        [Test]
        public void Service_And_Multiple_Named_Type_Scoped_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Scope);
            using (container.BeginScope())
            {
                VerifyNamedResolves();
            }
        }

        [Test]
        public void Service_And_Multiple_Named_Type_Singleton_Resolves_Types()
        {
            RegisterServiceAndNamedTypes(Lifetime.Singleton);
            VerifyNamedResolves();
        }

        [Test]
        public void Service_And_Multiple_Named_Instances_Resolves_Types()
        {
            container.RegisterInstance(typeof(IAbstraction), new Concrete(), "Concrete");
            container.RegisterInstance(typeof(IAbstraction), new AnotherConcrete(), "AnotherConcrete");
            VerifyNamedResolves();
        }

        [Test]
        public void Service_And_Multiple_Factories_Transient_Removes_Previous_Factory()
        {
            container.Register((Func<IContainer, IAbstraction>)(c => new Concrete()));
            container.Register((Func<IContainer, IAbstraction>)(c => new AnotherConcrete()));
            Assert.That(container.GetRegistered(typeof(IAbstraction)).ToArray(), Has.Length.EqualTo(1));
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<AnotherConcrete>());
        }
    }

    public interface IAbstraction
    {

    }

    public class Concrete : IAbstraction
    {

    }

    public class AnotherConcrete : IAbstraction
    {

    }
}
