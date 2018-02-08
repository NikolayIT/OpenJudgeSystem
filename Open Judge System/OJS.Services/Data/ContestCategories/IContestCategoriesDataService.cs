namespace OJS.Services.Data.ContestCategories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestCategoriesDataService : IService
    {
        IQueryable<ContestCategory> GetAll();

        IQueryable<ContestCategory> GetByLecturer(string lecturerId);
    }
}