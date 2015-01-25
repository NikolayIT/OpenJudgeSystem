namespace OJS.Web.Tests.Controllers.Contests.CompeteControllerTests
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using NUnit.Framework;

    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.Controllers;

    [TestFixture]
    public class CompeteControllerBaseTestsClass : BaseWebTests
    {
        protected readonly string DefaultCompetePassword = "competePassword";

        protected readonly string DefaultPracticePassword = "practicePassword";

        protected readonly bool IsPractice = false;

        protected readonly bool IsCompete = true;

        protected readonly Random RandomGenerator = new Random();

        public CompeteControllerBaseTestsClass()
        {
            this.FakeUserProfile = new UserProfile
            {
                UserName = "fake profile",
                Email = "test@test.com"
            };

            this.EmptyOjsData.Users.Add(this.FakeUserProfile);
            this.EmptyOjsData.SaveChanges();

            this.InactiveContestOptions = new ContestInitializationOptions();

            this.ActiveContestNoPasswordOptions = new ContestInitializationOptions
            {
                Enabled = true
            };

            this.ActiveContestWithPasswordOptions = new ContestInitializationOptions
            {
                Enabled = true,
                HasPassword = true
            };

            this.ActiveContestWithPasswordAndQuestionsOptions = new ContestInitializationOptions
            {
                Enabled = true,
                HasPassword = true,
                HasQuestions = true
            };

            this.ActiveContestWithQuestionsOptions = new ContestInitializationOptions
            {
                Enabled = true,
                HasQuestions = true
            };
        }

        protected UserProfile FakeUserProfile { get; set; }

        protected CompeteController CompeteController { get; set; }

        protected ControllerContext ControllerContext { get; set; }

        #region Contest initialization options
        protected ContestInitializationOptions InactiveContestOptions { get; set; }

        protected ContestInitializationOptions ActiveContestNoPasswordOptions { get; set; }

        protected ContestInitializationOptions ActiveContestWithPasswordOptions { get; set; }

        protected ContestInitializationOptions ActiveContestWithPasswordAndQuestionsOptions { get; set; }

        protected ContestInitializationOptions ActiveContestWithQuestionsOptions { get; set; }
        #endregion

        [SetUp]
        public virtual void TestInitialize()
        {
            this.InitializeController();
        }

        protected void InitializeController()
        {
            this.CompeteController = new CompeteController(this.EmptyOjsData, this.FakeUserProfile);

            this.ControllerContext = new ControllerContext(
                                                        this.MockHttpContextBase(),
                                                        new RouteData(),
                                                        this.CompeteController);
        }

        protected virtual Contest CreateAndSaveContest(string name, ContestInitializationOptions compete, ContestInitializationOptions practice)
        {
            var contestQuestions = new List<ContestQuestion>();

            if (compete.HasQuestions || practice.HasQuestions)
            {
                contestQuestions.Add(new ContestQuestion
                {
                    AskOfficialParticipants = compete.HasQuestions,
                    AskPracticeParticipants = practice.HasQuestions,
                    Text = "SampleQuestion"
                });
            }

            var contest = new Contest
            {
                Name = name,
                PracticeStartTime = practice.Enabled ? (DateTime?)new DateTime(2000, 1, 1) : null,
                PracticePassword = practice.HasPassword ? this.DefaultPracticePassword : null,
                StartTime = compete.Enabled ? (DateTime?)new DateTime(2000, 1, 1) : null,
                ContestPassword = compete.HasPassword ? this.DefaultCompetePassword : null,
                Questions = contestQuestions,
                IsVisible = true
            };

            this.EmptyOjsData.Contests.Add(contest);
            this.EmptyOjsData.SaveChanges();

            return contest;
        }
    }
}