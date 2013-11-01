namespace OJS.Tools.OldDatabaseMigration.Copiers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using OJS.Data;
    using OJS.Data.Models;

    internal sealed class ContestsCopier : ICopier
    {
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. C++ and C# in variable names should be cPlusPlus and cSharp.")]
        public void Copy(OjsDbContext context, TelerikContestSystemEntities oldDb)
        {
            var cSharpSubmissionType = context.SubmissionTypes.FirstOrDefault(x => x.Name == "C# code");
            var cPlusPlusSubmissionType = context.SubmissionTypes.FirstOrDefault(x => x.Name == "C++ code");
            var javaScriptSubmissionType = context.SubmissionTypes.FirstOrDefault(x => x.Name == "JavaScript code (NodeJS)");

            foreach (var oldContest in oldDb.Contests)
            {
                var contest = new Contest
                                  {
                                      OldId = oldContest.Id,
                                      CreatedOn = oldContest.AddedOn,
                                      PreserveCreatedOn = true,
                                      StartTime = oldContest.ActiveFrom,
                                      EndTime = oldContest.ActiveTo,
                                      ContestPassword = oldContest.Password,
                                      PracticePassword = oldContest.Password,
                                      OrderBy = oldContest.Order,
                                      Name = oldContest.Name.Trim(),
                                      IsVisible = oldContest.IsVisible,
                                      LimitBetweenSubmissions = oldContest.SubmissionsTimeLimit,
                                  };

                // Practice times
                if (!oldContest.ActiveFrom.HasValue && !oldContest.ActiveTo.HasValue)
                {
                    contest.PracticeStartTime = DateTime.Now;
                    contest.PracticeEndTime = null;
                }
                else if (oldContest.CanBePracticedAfterContest)
                {
                    contest.PracticeStartTime = oldContest.ActiveTo;
                    contest.PracticeEndTime = null;
                }
                else if (oldContest.CanBePracticedDuringContest)
                {
                    contest.PracticeStartTime = oldContest.ActiveFrom;
                    contest.PracticeEndTime = null;
                }
                else
                {
                    contest.PracticeStartTime = null;
                    contest.PracticeEndTime = null;
                }

                // Contest category
                var categoryName = oldContest.ContestType.Name;
                var category = context.ContestCategories.FirstOrDefault(x => x.Name == categoryName);
                contest.Category = category;

                // Contest question
                if (oldContest.Question != null)
                {
                    var question = new ContestQuestion
                                       {
                                           AskOfficialParticipants = true,
                                           AskPracticeParticipants = true,
                                           Text = oldContest.Question.Trim(),
                                           Type = ContestQuestionType.Default,
                                       };

                    if (oldContest.Answers != null)
                    {
                        var answers = oldContest.Answers.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var answerText in answers)
                        {
                            if (!string.IsNullOrWhiteSpace(answerText))
                            {
                                var answer = new ContestQuestionAnswer { Text = answerText.Trim() };
                                question.Answers.Add(answer);
                            }
                        }
                    }

                    contest.Questions.Add(question);
                }

                // Contest submission types
                if (oldContest.ContestType.AllowCSharpCode)
                {
                    contest.SubmissionTypes.Add(cSharpSubmissionType);
                }

                if (oldContest.ContestType.AllowCPlusPlusCode)
                {
                    contest.SubmissionTypes.Add(cPlusPlusSubmissionType);
                }

                if (oldContest.ContestType.AllowJavaScriptCode)
                {
                    contest.SubmissionTypes.Add(javaScriptSubmissionType);
                }

                context.Contests.Add(contest);
                Console.WriteLine(contest);
            }

            context.SaveChanges();
        }
    }
}