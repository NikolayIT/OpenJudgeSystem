namespace OJS.Services.Business.Contests
{
    using OJS.Services.Data.Contests;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IContestsDataService contestsData;

        public ContestsBusinessService(IContestsDataService contestsData) =>
            this.contestsData = contestsData;

        public bool IsContestIpValidByIdAndIp(int contestId, string ip) =>
            this.contestsData.IsOnlineById(contestId) ||
            this.contestsData.HasValidIpByIdAndIpValue(contestId, ip);
    }
}