namespace OJS.Services.Business.Contests
{
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
    }
}