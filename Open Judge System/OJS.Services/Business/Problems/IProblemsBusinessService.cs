namespace OJS.Services.Business.Problems
{
    using OJS.Services.Common;

    public interface IProblemsBusinessService : IService
    {
        void RetestById(int id);
    }
}