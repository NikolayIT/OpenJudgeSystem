namespace OJS.Data.Tests.News
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Tests.Common;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Contracts;
    using OJS.Tests.Common.DataFakes;

    [TestClass]
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
