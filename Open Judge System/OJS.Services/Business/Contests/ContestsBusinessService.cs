namespace OJS.Services.Business.Contests
{
    using System;
    using System.Linq;

    using OJS.Services.Data.Contests;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IContestsDataService contestsData;

        public ContestsBusinessService(IContestsDataService contestsData) =>
            this.contestsData = contestsData;

        public bool IsContestIpValidByIdAndIp(int contestId, string ip) =>
            this.contestsData
                .GetByIdQuery(contestId)
                .Any(c => !c.AllowedIps.Any() || c.AllowedIps.Any(ai => ai.Ip.Value == ip));

        public bool CanUserCompeteByContestUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false)
        {
            var contest = this.contestsData.GetById(contestId);

            var isUserAdminOrLecturerInContest = isAdmin || this.contestsData
                .IsUserLecturerInByContestIdAndUserId(contestId, userId);

            if (contest.CanBeCompeted)
            {
                return true;
            }

            if (isUserAdminOrLecturerInContest && allowToAdminAlways)
            {
                return true;
            }

            if (isUserAdminOrLecturerInContest && (contest.IsActive || contest.StartTime >= DateTime.Now))
            {
                return true;
            }

            if (contest.IsOnline)
            {
                return contest.Participants.Any(p =>
                    p.UserId == userId &&
                    p.IsOfficial &&
                    p.ContestEndTime >= DateTime.Now);
            }

            return false;
        }
    }
}