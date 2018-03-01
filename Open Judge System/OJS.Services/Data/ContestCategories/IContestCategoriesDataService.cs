namespace OJS.Services.Data.ContestCategories
{
    using OJS.Services.Common;

    public interface IContestCategoriesDataService : IService
    {
        string GetNameById(int id);
    }
}