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

        public string GetNameById(int id) =>
            this.contestCategories
                .All()
                .Where(cc => cc.Id == id)
                .Select(cc => cc.Name)
                .FirstOrDefault();
    }
}