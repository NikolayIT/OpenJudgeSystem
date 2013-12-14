namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using OJS.Data;

    public class RedirectsController : BaseController
    {
        public static List<KeyValuePair<string, string>> OldSystemRedirects =
            new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Contest/List", "/Contests"),
            };

        public RedirectsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index(int id)
        {
            return RedirectPermanent(OldSystemRedirects[id].Value);
        }
    }
}