namespace OJS.Services.Business.Contests
{
    using System;
    using System.Linq;
    using OJS.Data.Models;
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

        public bool CanUserCompeteByContestUserAndIsAdminOrLecturer(Contest contest, string userId, bool isAdminOrLecturer)
        {
            if (contest.CanBeCompeted)
            {
                return true;
            }

            if (isAdminOrLecturer && (contest.IsActive || contest.StartTime >= DateTime.Now))
            {
                return true;
            }

            if (contest.IsOnline)
            {
                return contest.Participants.Any(p =>
                    p.UserId == userId &&
                    p.IsOfficial &&
                    p.ContestEndTime > DateTime.Now);
            }

            return false;
        }

        public bool CanUserCompeteByContestUserAndIsAdminOrLecturer(int contestId, string userId, bool isAdminOrLecturer)
        {
            var contest = this.contestsData.GetById(contestId);
            return this.CanUserCompeteByContestUserAndIsAdminOrLecturer(contest, userId, isAdminOrLecturer);
        }
    }
}