namespace OJS.Services.Business.Contests
{
    using OJS.Services.Common;

    public interface IContestsBusinessService : IService
    {
        bool IsContestIpValidByContestAndIp(int contestId, string ip);

        /// <summary>
        /// Determines if a user can compete in a contest, depending of his role and the contest type
        /// </summary>
        /// <param name="contestId">The id of the contest</param>
        /// <param name="userId">The id of the user</param>
        /// <param name="isAdmin">Is the user administrator in the system</param>
        /// <param name="allowToAdminAlways">If true, and the user is admin he will always be able to compete</param>
        bool CanUserCompeteByContestByUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false);

        ServiceResult TransferParticipantsToPracticeById(int contestId);

        void DeleteById(int id);
    }
}