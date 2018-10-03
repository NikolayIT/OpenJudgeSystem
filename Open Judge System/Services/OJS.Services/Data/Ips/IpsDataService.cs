namespace OJS.Services.Data.Ips
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class IpsDataService : IIpsDataService
    {
        private readonly IEfGenericRepository<Ip> ips;

        public IpsDataService(IEfGenericRepository<Ip> ips) =>
            this.ips = ips;

        public Ip GetByValue(string value) =>
            this.ips
                .All()
                .FirstOrDefault(ip => ip.Value == value);
    }
}