namespace OJS.Services.Business.Contests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;

        public ContestsBusinessService(IEfDeletableEntityRepository<Contest> contests) =>
            this.contests = contests;

        public bool IsContestIpValidByIdAndIp(int contestId, string ip) =>
            this.contests
                .All()
                .Any(c => c.Id == contestId && (!c.AllowedIps.Any() || c.AllowedIps.Any(ai => ai.Ip.Value == ip)));
    }
}