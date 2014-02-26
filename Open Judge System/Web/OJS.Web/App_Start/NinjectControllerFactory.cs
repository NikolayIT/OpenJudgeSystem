namespace OJS.Web
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Ninject;

    using OJS.Data;

    public class NinjectControllerFactory : DefaultControllerFactory, IDisposable
    {
        private readonly IKernel ninjectKernel;

        public NinjectControllerFactory()
        {
            this.ninjectKernel = new StandardKernel();
            this.AddBindings();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ninjectKernel.Dispose();
            }
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return (controllerType == null) ? null : (IController)this.ninjectKernel.Get(controllerType);
        }

        private void AddBindings()
        {
            this.ninjectKernel.Bind<IOjsData>().To<OjsData>();
        }
    }
}