using Castle.MicroKernel;
using Castle.MicroKernel.SubSystems.Naming;
using Castle.Windsor;

namespace Our.Umbraco.Containers.Castle
{
    public static class RemovalExtensions
    {
        public static IWindsorContainer RemoveHandler<T>(this IWindsorContainer container)
        {
            container.EnsureDeletableNamingSubSystem().RemoveHandler<T>();
            return container;
        }

        public static IWindsorContainer RemoveHandler(this IWindsorContainer container, string name)
        {
            container.EnsureDeletableNamingSubSystem().RemoveHandler(name);
            return container;
        }

        private static RemovableNamingSubSystem EnsureDeletableNamingSubSystem(
            this IWindsorContainer container)
        {
            var key = SubSystemConstants.NamingKey;
            var naming = container.Kernel.GetSubSystem(key) as INamingSubSystem;
            var removableNaming = naming as RemovableNamingSubSystem;
            if (removableNaming != null) return removableNaming;
            removableNaming = new RemovableNamingSubSystem(naming);
            container.Kernel.AddSubSystem(key, removableNaming);
            return removableNaming;
        }
    }
}
