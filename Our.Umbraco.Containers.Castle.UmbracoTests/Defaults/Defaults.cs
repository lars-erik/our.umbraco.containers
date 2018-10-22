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
        public void Just_The_Type_Transient_Is_Irrelevant_Since_No_Alternative()
        {
            container.Register(typeof(Concrete));
            container.Register(typeof(AnotherConcrete));
            Assert.That(container.GetInstance(typeof(Concrete)), Is.InstanceOf<Concrete>());
        }

        [Test]
        public void Service_And_Type_Transient_First_Is_Default()
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), Lifetime.Transient);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), Lifetime.Transient);
            Assert.That(container.GetInstance(typeof(IAbstraction)), Is.InstanceOf<Concrete>());
        }

        [Test]
        public void Service_And_Type_Singleton_Last_Is_Default()
        {
            container.Register(typeof(IAbstraction), typeof(Concrete), Lifetime.Singleton);
            container.Register(typeof(IAbstraction), typeof(AnotherConcrete), Lifetime.Singleton);
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
