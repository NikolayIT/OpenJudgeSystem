namespace OJS.Web.Areas.Administration.Providers.Contracts
{
    using System.Collections.Generic;

    using OJS.Data.Models;

    public interface INewsProvider
    {
        IEnumerable<News> FetchNews();
    }
}
