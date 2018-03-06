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

                this.problemGroups.Delete(problemGroup);
                this.problemGroups.SaveChanges();
            }

            return ServiceResult.Success;
        }

        public void DeleteByContest(int contestId) =>
            this.problemGroups.Delete(pg => pg.ContestId == contestId &&
                !pg.IsDeleted &&
                pg.Problems.All(p => p.IsDeleted));
    }
}