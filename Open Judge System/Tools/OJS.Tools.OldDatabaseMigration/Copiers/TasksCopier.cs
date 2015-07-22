namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class TasksCopier : ICopier
    {
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            context.Configuration.AutoDetectChangesEnabled = false;
            foreach (var task in oldDb.Tasks.OrderBy(x => x.Id).Include(x => x.Contest1).ToList())
            {
                var contest = context.Contests.FirstOrDefault(x => x.OldId == task.Contest);
                var problem = new Problem
                                  {
                                      OldId = task.Id,
                                      Contest = contest,
                                      Name = task.Name,
                                      MaximumPoints = (short)task.MaxPoints,
                                      TimeLimit = task.TimeLimit,
                                      MemoryLimit = (int)task.MemoryLimit * 2, // Multiplying memory limit by 2 because the previous worker didn't respect the memory limit
                                      OrderBy = 0,
                                      ShowResults = task.Contest1.ShowResults,
                                      CreatedOn = task.AddedOn,
                                      PreserveCreatedOn = true,
                                  };

                if (task.DescriptionLink != null || (task.Description != null && task.Description.Length != 0))
                {
                    var resource = new ProblemResource
                                              {
                                                  CreatedOn = task.AddedOn,
                                                  PreserveCreatedOn = true,
                                                  Name = "Условие на задачата",
                                                  Type = ProblemResourceType.ProblemDescription,
                                              };

                    if (task.DescriptionLink != null)
                    {
                        if (task.Id == 368)
                        {
                            task.DescriptionLink =
                                "http://downloads.academy.telerik.com/svn/oop/Lectures/9.%20Exam%20Preparation/DocumentSystem-Skeleton.rar";
                        }

                        if (task.Id == 369)
                        {
                            task.DescriptionLink =
                                "http://downloads.academy.telerik.com/svn/oop/Lectures/9.%20Exam%20Preparation/AcademyGeometry-Skeleton.zip";
                        }

                        var web = new WebClient();
                        var data = web.DownloadData(task.DescriptionLink);
                        resource.File = data;
                        resource.FileExtension = task.DescriptionLink.GetFileExtension();
                    }
                    else
                    {
                        resource.File = task.Description;
                        resource.FileExtension = task.DescriptionFormat;
                    }

                    problem.Resources.Add(resource);
                }

                switch (task.Checker1.Name)
                {
                    case "Exact":
                        problem.Checker = context.Checkers.FirstOrDefault(x => x.Name == "Exact");
                        break;
                    case "Trim":
                        problem.Checker = context.Checkers.FirstOrDefault(x => x.Name == "Trim");
                        break;
                    case "Sort":
                        problem.Checker = context.Checkers.FirstOrDefault(x => x.Name == "Sort lines");
                        break;
                    default:
                        problem.Checker = null;
                        break;
                }

                context.Problems.Add(problem);
            }

            context.SaveChanges();
            context.Configuration.AutoDetectChangesEnabled = true;
        }
    }
}

/*
Useless properties "RequiresAllTests, RequiresZeroTests, AddedBy and JudgeNow (every problem will be judged immediately).
*/