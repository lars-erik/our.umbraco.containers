using System;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Our.Umbraco.Containers.Castle
{
    public class CastleWindsorComponent : IUmbracoUserComponent
    {
        public void Compose(Composition composition)
        {

        }

        public Type InitializerType => typeof(CastleInitializer);

        public class CastleInitializer : IComponentInitializer
        {
            private readonly IContainer container;

            public CastleInitializer(IContainer container)
            {
                this.container = container;
            }

            public void Initialize()
            {
                //container.GetInstance<FilteredControllerFactoryCollectionBuilder>()
                //    .Insert<WindsorControllerFactory>();
            }
        }

        public void Terminate()
        {
        }
    }
}