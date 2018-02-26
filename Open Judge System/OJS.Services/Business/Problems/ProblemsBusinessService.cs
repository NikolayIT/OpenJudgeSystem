namespace OJS.Services.Business.Problems
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;

    using OJS.Services.Business.ProblemGroups;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.ProblemResources;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;
    using OJS.Services.Data.Tests;

    public class ProblemsBusinessService : IProblemsBusinessService
    {
        private readonly IContestsDataService contestsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IProblemsDataService problemsData;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemResourcesDataService problemResourcesData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;
        private readonly ITestsDataService testsData;
        private readonly ITestRunsDataService testRunsData;
        private readonly IProblemGroupsBusinessService problemGroupsBusiness;

        public ProblemsBusinessService(
            IContestsDataService contestsData,
            IParticipantScoresDataService participantScoresData,
            IProblemsDataService problemsData,
            IProblemGroupsDataService problemGroupsData,
            IProblemResourcesDataService problemResourcesData,
            ISubmissionsDataService submissionsData,
            ISubmissionsForProcessingDataService submissionsForProcessingData,
            ITestsDataService testsData,
            ITestRunsDataService testRunsData,
            IProblemGroupsBusinessService problemGroupsBusiness)
        {
            this.contestsData = contestsData;
            this.participantScoresData = participantScoresData;
            this.problemsData = problemsData;
            this.problemGroupsData = problemGroupsData;
            this.problemResourcesData = problemResourcesData;
            this.submissionsData = submissionsData;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.testsData = testsData;
            this.testRunsData = testRunsData;
            this.problemGroupsBusiness = problemGroupsBusiness;
        }

        public void RetestById(int id)
        {
            var submissionIds = this.submissionsData.GetIdsByProblem(id).ToList();

            using (var scope = new TransactionScope())
            {
                this.participantScoresData.DeleteAllByProblem(id);

                this.submissionsData.SetAllToUnprocessedByProblem(id);

                this.submissionsForProcessingData.AddOrUpdate(submissionIds);

                scope.Complete();
            }
        }

        public void DeleteById(int id)
        {
            this.problemResourcesData.DeleteByProblem(id);

            this.testRunsData.DeleteByProblem(id);

            this.testsData.DeleteByProblem(id);

            this.submissionsData.DeleteByProblem(id);

            this.problemsData.DeleteById(id);

            if (!this.problemsData.IsFromOnlineContestById(id))
            {
                this.problemGroupsBusiness.DeleteByProblem(id);
            }
        }

        public void DeleteByContest(int contestId)
        {
            this.problemResourcesData.DeleteByContest(contestId);

            this.testRunsData.DeleteByContest(contestId);

            this.testsData.DeleteByContest(contestId);

            this.submissionsData.DeleteByContest(contestId);

            this.problemsData.DeleteByContest(contestId);

            if (!this.contestsData.IsOnlineById(contestId))
            {
                var problemGroups = this.problemGroupsData
                    .GetAllByContest(contestId)
                    .Include(pg => pg.Problems);

                var problemGroupIdsMarkedForDeletion = new List<int>();

                foreach (var problemGroup in problemGroups)
                {
                    if (problemGroup.Problems.Any(p => !p.IsDeleted))
                    {
                        continue;
                    }

                    problemGroupIdsMarkedForDeletion.Add(problemGroup.Id);
                }

                this.problemGroupsData.DeleteByIds(problemGroupIdsMarkedForDeletion);
            }
        }
    }
}