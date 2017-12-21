namespace OJS.Services.Business.Contests
{
    using OJS.Services.Common;

    public interface IContestsBusinessService : IService
    {
        bool IsContestIpValidByIdAndIp(int contestId, string ip);
    }
}