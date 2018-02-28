namespace OJS.Services.Business.ProblemGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Common;
    using OJS.Services.Data.ProblemGroups;

    public class ProblemGroupsBusinessService : IProblemGroupsBusinessService
    {
        //TODO: Add to Resource
        private const string CannotDeleteProblemGroupWithProblems = "Cannot delete problem group with problems";

        private readonly IEfDeletableEntityRepository<ProblemGroup> problemGroups;
        private readonly IProblemGroupsDataService problemGroupsData;

        public ProblemGroupsBusinessService(
            IEfDeletableEntityRepository<ProblemGroup> problemGroups,
            IProblemGroupsDataService problemGroupsData)
        {
            this.problemGroups = problemGroups;
            this.problemGroupsData = problemGroupsData;
        }

        public ServiceResult DeleteById(int id)
        {
            var problemGroup = this.problemGroupsData.GetById(id);

            if (problemGroup != null)
            {
                if (problemGroup.Problems.Any(p => !p.IsDeleted))
                {
                    return new ServiceResult(CannotDeleteProblemGroupWithProblems);
                }

                this.Delete(problemGroup);
            }

            return ServiceResult.Success;
        }

        public void DeleteByContest(int contestId)
        {
            var problemGroupIds = this.problemGroupsData
                .GetAllByContest(contestId)
                .Where(pg => !pg.IsDeleted && pg.Problems.All(p => p.IsDeleted))
                .Select(pg => pg.Id)
                .ToList();

            if (problemGroupIds.Any())
            {
                this.problemGroups.Delete(pg => problemGroupIds.Contains(pg.Id));
            }
        }

        private void Delete(ProblemGroup problemGroup)
        {
            this.problemGroups.Delete(problemGroup);
            this.problemGroups.SaveChanges();
        }
    }
}