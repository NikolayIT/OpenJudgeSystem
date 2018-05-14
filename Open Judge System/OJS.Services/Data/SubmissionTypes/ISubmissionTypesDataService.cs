namespace OJS.Services.Data.SubmissionTypes
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionTypesDataService : IService
    {
        SubmissionType GetById(int id);

        IQueryable<SubmissionType> GetAll();

        IQueryable<SubmissionType> GetAllByProblem(int problemId);
    }
}