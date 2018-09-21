namespace OJS.Services.Data.ContestCategories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestCategoriesDataService : IContestCategoriesDataService
    {
        private readonly IEfDeletableEntityRepository<ContestCategory> contestCategories;

        public ContestCategoriesDataService(IEfDeletableEntityRepository<ContestCategory> contestCategories) =>
            this.contestCategories = contestCategories;

        public ContestCategory GetById(int id) => this.contestCategories.GetById(id);

        public IQueryable<ContestCategory> GetAllVisible() =>
            this.contestCategories
                .All()
                .Where(cc => cc.IsVisible);

        public IQueryable<ContestCategory> GetAllVisibleByLecturer(string lecturerId) =>
            this.GetAllVisible()
                .Where(cc =>
                    cc.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    cc.Contests.Any(c => c.Lecturers.Any(l => l.LecturerId == lecturerId)));

        public string GetNameById(int id) =>
            this.contestCategories
                .All()
                .Where(cc => cc.Id == id)
                .Select(cc => cc.Name)
                .FirstOrDefault();

        public bool HasContestsById(int id) =>
            this.GetAllVisible()
                .Where(cc => cc.Id == id)
                .Any(cc => cc.Contests.Any());
    }
}