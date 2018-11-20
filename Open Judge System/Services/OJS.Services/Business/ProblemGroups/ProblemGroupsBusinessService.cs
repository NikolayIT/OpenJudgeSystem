namespace OJS.Services.Business.ProblemGroups
{
    using System.Data.Entity;
    using System.Linq;

    using MissingFeatures;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.SubmissionTypes;

    using Resource = Resources.Services.ProblemGroups.ProblemGroupsBusiness;
    using SharedResource = Resources.Contests.ContestsGeneral;

    public class ProblemGroupsBusinessService : IProblemGroupsBusinessService
    {
        private readonly IEfDeletableEntityRepository<ProblemGroup> problemGroups;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IContestsDataService contestsData;
        private readonly ISubmissionTypesDataService submissionTypesData;

        public ProblemGroupsBusinessService(
            IEfDeletableEntityRepository<ProblemGroup> problemGroups,
            IProblemGroupsDataService problemGroupsData,
            IContestsDataService contestsData,
            ISubmissionTypesDataService submissionTypesData)
        {
            this.problemGroups = problemGroups;
            this.problemGroupsData = problemGroupsData;
            this.contestsData = contestsData;
            this.submissionTypesData = submissionTypesData;
        }

        public ServiceResult DeleteById(int id)
        {
            var problemGroup = this.problemGroupsData.GetById(id);

            if (problemGroup != null)
            {
                if (problemGroup.Problems.Any(p => !p.IsDeleted))
                {
                    return new ServiceResult(Resource.Cannot_delete_problem_group_with_problems);
                }

                this.problemGroups.Delete(problemGroup);
                this.problemGroups.SaveChanges();
            }

            return ServiceResult.Success;
        }

        public ServiceResult CopyAllToContestBySourceAndDestinationContest(
            int sourceContestId,
            int destinationContestId)
        {
            if (sourceContestId == destinationContestId)
            {
                return new ServiceResult(Resource.Cannot_copy_problem_groups_into_same_contest);
            }

            if (!this.contestsData.ExistsById(destinationContestId))
            {
                return new ServiceResult(SharedResource.Contest_not_found);
            }

            if (this.contestsData.IsActiveById(destinationContestId))
            {
                return new ServiceResult(Resource.Cannot_copy_problem_groups_into_active_contest);
            }

            var sourceContestProblemGroups = this.problemGroupsData
                .GetAllByContest(sourceContestId)
                .AsNoTracking()
                .Include(pg => pg.Problems.Select(p => p.Tests))
                .Include(pg => pg.Problems.Select(p => p.Resources))
                .ToList();

            sourceContestProblemGroups
                .ForEach(pg => this.CopyProblemGroupToContest(pg, destinationContestId));

            return ServiceResult.Success;
        }

        private void CopyProblemGroupToContest(ProblemGroup problemGroup, int contestId)
        {
            problemGroup.Contest = null;
            problemGroup.ContestId = contestId;

            problemGroup.Problems
                .ForEach(p => p.SubmissionTypes = this.submissionTypesData.GetAllByProblem(p.Id).ToList());

            this.problemGroupsData.Add(problemGroup);
        }
    }
}