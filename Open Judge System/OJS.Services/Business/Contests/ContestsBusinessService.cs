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

        public bool IsContestIpValidByContestAndIp(int contestId, string ip) =>
            this.contestsData
                .GetByIdQuery(contestId)
                .Any(c => !c.AllowedIps.Any() || c.AllowedIps.Any(ai => ai.Ip.Value == ip));

        public bool CanUserCompeteByContestByUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false)
        {
            var contest = this.contestsData.GetById(contestId);

            var isUserAdminOrLecturerInContest = isAdmin || this.contestsData
                .IsUserLecturerInByContestAndUser(contestId, userId);

            if (contest.IsOnline && !isUserAdminOrLecturerInContest)
            {
                var participant = contest.Participants.FirstOrDefault(p => p.UserId == userId && p.IsOfficial);

                if (participant == null)
                {
                    return contest.CanBeCompeted;
                }

                return participant.ContestEndTime >= DateTime.Now;
            }

            if (contest.CanBeCompeted || (isUserAdminOrLecturerInContest && allowToAdminAlways))
            {
                return true;
            }

            if (isUserAdminOrLecturerInContest && (contest.IsActive || contest.StartTime >= DateTime.Now))
            {
                return true;
            }

            return false;
        }
    }
}