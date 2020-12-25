using Consul;
using System;

namespace TeaMaki.Orders.Services
{
    public class ConsulRegistryService : IConsulRegistryService
    {
        private readonly IConsulClient _consulClient;
        public const string ServiceName = "Product";

        public ConsulRegistryService(IConsulClient consulClient)
        {
            _consulClient = consulClient;

        }

        public Uri GetService()
        {
            var serviceQueryResult = _consulClient.Health.Service(ServiceName).Result;

            if (serviceQueryResult != null 
                && serviceQueryResult.Response != null 
                && serviceQueryResult.Response.Length > 0)
            {
                var service = serviceQueryResult.Response[0];
                return new Uri($"http://{service.Service.Address}:{service.Service.Port}");
            }

            return null;
        }

    }
}
