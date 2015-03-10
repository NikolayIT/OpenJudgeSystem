namespace OJS.Web
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    internal class ViewEnginesConfig
    {
        private readonly ICollection<IViewEngine> viewEngines;

        public ViewEnginesConfig(ICollection<IViewEngine> viewEngines)
        {
            this.viewEngines = viewEngines;
        }

        public void RegisterViewEngines(params IViewEngine[] viewEnginesToRegister)
        {
            this.viewEngines.Clear();

            foreach (var viewEngine in viewEnginesToRegister)
            {
                this.viewEngines.Add(viewEngine);
            }
        }
    }
}