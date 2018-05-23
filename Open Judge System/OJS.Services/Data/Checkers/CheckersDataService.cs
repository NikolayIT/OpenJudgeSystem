namespace OJS.Services.Data.Checkers
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class CheckersDataService : ICheckersDataService
    {
        private readonly IEfDeletableEntityRepository<Checker> checkers;

        public CheckersDataService(IEfDeletableEntityRepository<Checker> checkers) =>
            this.checkers = checkers;

        public Checker GetByName(string name) =>
            this.GetAll()
                .FirstOrDefault(ch => ch.Name == name);

        public IQueryable<Checker> GetAll() => this.checkers.All();
    }
}