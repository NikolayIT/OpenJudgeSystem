namespace OJS.Data.Tests.News
{
    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Tests.Common;

    [TestFixture]
    public class TestNewsBaseData : TestClassBase
    {
        protected void PopulateEmptyDataBaseWithNews()
        {
            this.InitializeEmptyOjsData();

            this.EmptyOjsData.News.Add(new News
            {
                Title = "Created",
                Content = "Test news",
                Author = "Author",
                Source = "Source",
                IsVisible = true,
            });

            this.EmptyOjsData.SaveChanges();
        }
    }
}
