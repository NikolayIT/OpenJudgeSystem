namespace OJS.Services.Business.Problems
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Business.ProblemGroups;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.ProblemResources;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.TestRuns;

    using IsolationLevel = System.Transactions.IsolationLevel;

    public class ProblemsBusinessService : IProblemsBusinessService
    {
        // TODO: Add messages to resources
        private const string InvalidProblemOrContestErrorMessage = "Invalid problem or contest";
        private const string CannotCopyProblemInActiveContestErrorMessage = "Cannot copy problems into Active Contest";

        private readonly IEfDeletableEntityRepository<Problem> problems;
        private readonly IContestsDataService contestsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IProblemsDataService problemsData;
        private readonly IProblemResourcesDataService problemResourcesData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;
        private readonly ITestRunsDataService testRunsData;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemGroupsBusinessService problemGroupsBusiness;

        public ProblemsBusinessService(
            IEfDeletableEntityRepository<Problem> problems,
            IContestsDataService contestsData,
            IParticipantScoresDataService participantScoresData,
            IProblemsDataService problemsData,
            IProblemResourcesDataService problemResourcesData,
            ISubmissionsDataService submissionsData,
            ISubmissionsForProcessingDataService submissionsForProcessingData,
            ITestRunsDataService testRunsData,
            IProblemGroupsDataService problemGroupsData,
            IProblemGroupsBusinessService problemGroupsBusiness)
        {
            this.problems = problems;
            this.contestsData = contestsData;
            this.participantScoresData = participantScoresData;
            this.problemsData = problemsData;
            this.problemResourcesData = problemResourcesData;
            this.submissionsData = submissionsData;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.testRunsData = testRunsData;
            this.problemGroupsData = problemGroupsData;
            this.problemGroupsBusiness = problemGroupsBusiness;
        }

        public void RetestById(int id)
        {
            var submissionIds = this.submissionsData.GetIdsByProblem(id).ToList();

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.RepeatableRead))
            {
                this.participantScoresData.DeleteAllByProblem(id);

                this.submissionsData.SetAllToUnprocessedByProblem(id);

                this.submissionsForProcessingData.AddOrUpdateBySubmissionIds(submissionIds);

                scope.Complete();
            }
        }

        public void DeleteById(int id)
        {
            var problem = this.problemsData
                .GetByIdQuery(id)
                .Select(p => new
                {
                    p.ProblemGroupId,
                    p.ProblemGroup.ContestId
                })
                .FirstOrDefault();

            if (problem == null)
            {
                return;
            }

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.RepeatableRead))
            {
                this.testRunsData.DeleteByProblem(id);

                this.problemResourcesData.DeleteByProblem(id);

                this.submissionsData.DeleteByProblem(id);

                this.problems.Delete(id);
                this.problems.SaveChanges();

                if (!this.contestsData.IsOnlineById(problem.ContestId))
                {
                    this.problemGroupsBusiness.DeleteById(problem.ProblemGroupId.Value);
                }

                scope.Complete();
            }
        }

        public void DeleteByContest(int contestId) =>
            this.problemsData
                .GetAllByContest(contestId)
                .Select(p => p.Id)
                .ToList()
                .ForEach(this.DeleteById);

        public ServiceResult CopyToContestByIdAndContest(int id, int contestId)
        {
            var problem = this.GetProblemWithModelsForCopy(id);

            if (problem == null || !this.contestsData.ExistsById(contestId))
            {
                return new ServiceResult(InvalidProblemOrContestErrorMessage);
            }

            var problemNewOrderBy = this.problemsData.GetNewOrderByContest(contestId);

            return this.CopyProblem(problem, contestId, problemNewOrderBy);
        }

        public ServiceResult CopyToProblemGroupByIdAndProblemGroup(int id, int problemGroupId)
        {
            var problem = this.GetProblemWithModelsForCopy(id);

            if (problem == null || !this.problemGroupsData.ExistsById(problemGroupId))
            {
                return new ServiceResult(InvalidProblemOrContestErrorMessage);
            }

            var problemNewOrderBy = this.problemsData.GetNewOrderByProblemGroup(problemGroupId);

            return this.CopyProblem(problem, problem.ProblemGroup.ContestId, problemNewOrderBy, problemGroupId);
        }

        private ServiceResult CopyProblem(Problem problem, int contestId, int newOrderBy, int? problemGroupId = null)
        {
            if (this.contestsData.IsActiveById(contestId))
            {
                return new ServiceResult(CannotCopyProblemInActiveContestErrorMessage);
            }

            if (problemGroupId.HasValue)
            {
                problem.ProblemGroupId = problemGroupId;
            }
            else
            {
                problem.ProblemGroup = new ProblemGroup
                {
                    ContestId = contestId,
                    OrderBy = newOrderBy
                };
            }

            problem.Id = default(int);
            problem.ModifiedOn = null;
            problem.OrderBy = newOrderBy;

            this.problemsData.Add(problem);
            return ServiceResult.Success;
        }

        private Problem GetProblemWithModelsForCopy(int problemId) =>
            this.problemsData
                .GetByIdQuery(problemId)
                .AsNoTracking()
                .Include(p => p.Tests)
                .Include(p => p.Resources)
                .Include(p => p.SubmissionTypes)
                .SingleOrDefault();
    }
}