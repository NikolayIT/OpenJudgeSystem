namespace OJS.Services.Data.SubmissionTypes
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionTypesDataService : ISubmissionTypesDataService
    {
        private readonly IEfGenericRepository<SubmissionType> submissionTypes;

        public SubmissionTypesDataService(IEfGenericRepository<SubmissionType> submissionTypes) =>
            this.submissionTypes = submissionTypes;

        public SubmissionType GetById(int id) => this.submissionTypes.GetById(id);

        public IQueryable<SubmissionType> GetAll() => this.submissionTypes.All();

        public IQueryable<SubmissionType> GetAllByProblem(int problemId) =>
            this.GetAll()
                .Where(st => st.Problems
                    .Any(p => p.Id == problemId));
    }
}