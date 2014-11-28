namespace OJS.Data.Contracts
{
    using System.Linq;

    public interface IDeletableEntityRepository<T> : IRepository<T> where T : class
    {
        IQueryable<T> AllWithDeleted();

        void HardDelete(T entity);
    }
}
