namespace OJS.Data.Tests.News
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Tests.Common;
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
