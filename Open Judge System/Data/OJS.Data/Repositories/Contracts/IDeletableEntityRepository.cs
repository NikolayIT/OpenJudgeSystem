namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    public interface IDeletableEntityRepository<out T>
    {
        IQueryable<T> AllWithDeleted();
    }
}
