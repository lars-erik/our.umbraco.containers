using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;

namespace Our.Umbraco.Containers.Castle
{
    /// <summary>
    /// Possibly not needed at all?
    /// </summary>
    public class WindsorControllerFactory : DefaultControllerFactory, IFilteredControllerFactory
    {
        private readonly IContainer container;

        public WindsorControllerFactory(IContainer container)
        {
            this.container = container;
        }

        public bool CanHandle(RequestContext request)
        {
            return true;
        }

        public override void ReleaseController(IController controller)
        {
            ((WindsorContainer)container.ConcreteContainer).Kernel.ReleaseComponent(controller);
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, string.Format("The controller for path '{0}' could not be found.", requestContext.HttpContext.Request.Path));
            }
            return (IController)container.GetInstance(controllerType);
        }
    }
}