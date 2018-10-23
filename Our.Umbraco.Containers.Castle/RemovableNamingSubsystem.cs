using System;
using System.Linq;
using Castle.MicroKernel.SubSystems.Naming;

namespace Our.Umbraco.Containers.Castle
{
    public class RemovableNamingSubSystem : DefaultNamingSubSystem
    {
        public RemovableNamingSubSystem(INamingSubSystem namingSubSystem)
        {
            namingSubSystem.GetAllHandlers().ToList().ForEach(Register);
        }

        public void RemoveHandler<T>()
        {
            var type = typeof(T);
            var invalidate = false;
            using (@lock.ForWriting())
            {
                if (name2Handler.ContainsKey(type.FullName))
                {
                    invalidate = true;
                    name2Handler.Remove(type.FullName);
                }
                if (service2Handler.ContainsKey(type))
                {
                    invalidate = true;
                    service2Handler.Remove(type);
                }
                if (invalidate) InvalidateCache();
            }
        }

        public void RemoveHandler(string name)
        {
            var invalidate = false;
            using (@lock.ForWriting())
            {
                if (name2Handler.ContainsKey(name))
                {
                    invalidate = true;
                    name2Handler.Remove(name);
                }
                else
                {
                    throw new InvalidOperationException("No component named " + name);
                }
                if (invalidate) InvalidateCache();
            }
        }
    }
}
