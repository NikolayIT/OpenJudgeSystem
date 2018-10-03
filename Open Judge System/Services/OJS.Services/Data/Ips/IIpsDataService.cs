namespace OJS.Services.Data.Ips
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IIpsDataService : IService
    {
        Ip GetByValue(string value);
    }
}