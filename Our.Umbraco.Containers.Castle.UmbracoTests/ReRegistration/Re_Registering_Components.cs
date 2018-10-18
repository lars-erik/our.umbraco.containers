using NUnit.Framework;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.Castle.UmbracoTests.ReRegistration
{
    [TestFixture]
    public class Re_Registering_Components
    {
        [Test]
        public void First_Default_Is_For_Singletons()
        {
            var container = new CastleContainer();
            container.Register<IAbstraction, Concrete>(Lifetime.Singleton);
            container.Register<IAbstraction, AnotherConcrete>(Lifetime.Singleton);
            var it = container.GetInstance<IAbstraction>();
            Assert.That(it, Is.InstanceOf<Concrete>());
        }

        [Test]
        public void Something_Is_Default_For_Singletons_Registered_With_Factory()
        {
            var container = new CastleContainer();
            container.Register<IAbstraction>(c => new Concrete(), Lifetime.Singleton);
            container.Register<IAbstraction>(c => new AnotherConcrete(), Lifetime.Singleton);
            var it = container.GetInstance<IAbstraction>();
            Assert.That(it, Is.InstanceOf<AnotherConcrete>());
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
