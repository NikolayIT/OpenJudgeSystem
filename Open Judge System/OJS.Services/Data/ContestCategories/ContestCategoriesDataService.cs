namespace OJS.Services.Data.ContestCategories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class CategoriesDataService : IContestCategoriesDataService
    {
        private readonly IEfDeletableEntityRepository<ContestCategory> contestCategories;

        public CategoriesDataService(IEfDeletableEntityRepository<ContestCategory> contestCategories) =>
            this.contestCategories = contestCategories;

        public IQueryable<ContestCategory> GetAll() => this.contestCategories.All();

        public IQueryable<ContestCategory> GetByLecturer(string lecturerId) =>
            this.GetAll().Where(cc =>
                cc.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                cc.Contests.Any(c => c.Lecturers.Any(l => l.LecturerId == lecturerId)));
    }
}