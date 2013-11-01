namespace OJS.Web.Tests.Contollers.NewsControllerTests
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data;
    using OJS.Tests.Common;
    using OJS.Web.Controllers;
    using System.Web.Mvc;

    using OJS.Web.ViewModels.News;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OJS.Data.Models;

    public class NewsTestBaseClass : TestClassBase
    {
        public NewsTestBaseClass()
        {
            for (int i = 0; i < 40; i++)
            {
                this.EmptyOjsData.News.Add(new News
                {
                    Title = "News Title " + i,
                    Author = "Author",
                    Source = "Source",
                    IsVisible = true,
                    Content = "News Content " + i
                });

                this.EmptyOjsData.News.Add(new News
                {
                    Title = "Not Visible News Title " + i,
                    Author = "Author",
                    Source = "Source",
                    IsVisible = false,
                    Content = "Not Visilbe News Content " + i
                });
            }

            this.EmptyOjsData.SaveChanges();

            for (int i = 0; i < 10; i++)
            {
                var id = this.EmptyOjsData.News.All().FirstOrDefault(x => x.IsVisible && !x.IsDeleted).Id;
                this.EmptyOjsData.News.Delete(id);
            }

            this.EmptyOjsData.SaveChanges();
        }
    }
}
