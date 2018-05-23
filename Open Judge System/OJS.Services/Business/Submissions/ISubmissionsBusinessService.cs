namespace OJS.Services.Business.Submissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsBusinessService : IService
    {
        IQueryable<Submission> GetAllForArchiving();

        void HardDeleteByIds(ICollection<int> ids);
    }
}