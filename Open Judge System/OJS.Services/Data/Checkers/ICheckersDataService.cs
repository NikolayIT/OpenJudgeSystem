namespace OJS.Services.Data.Checkers
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ICheckersDataService : IService
    {
        Checker GetByName(string name);

        IQueryable<Checker> GetAll();
    }
}