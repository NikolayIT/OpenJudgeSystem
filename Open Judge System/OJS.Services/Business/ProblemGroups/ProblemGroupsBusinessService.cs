namespace OJS.Services.Business.ProblemGroups
{
    using System.Linq;

    using OJS.Services.Common;
    using OJS.Services.Data.ProblemGroups;

    public class ProblemGroupsBusinessService : IProblemGroupsBusinessService
    {
        //TODO: Add to Resource
        private const string CannotDeleteProblemGroupWithProblems = "Cannot delete problem group with problems";

        private readonly IProblemGroupsDataService problemGroupsData;

        public ProblemGroupsBusinessService(IProblemGroupsDataService problemGroupsData) =>
            this.problemGroupsData = problemGroupsData;

        public ServiceResult DeleteById(int id)
        {
            var problemGroup = this.problemGroupsData.GetById(id);

            if (problemGroup != null)
            {
                if (!problemGroup.Problems.All(p => p.IsDeleted))
                {
                    return new ServiceResult(CannotDeleteProblemGroupWithProblems);
                }

                this.problemGroupsData.Delete(problemGroup);
            }

            return ServiceResult.Success;
        }
    }
}