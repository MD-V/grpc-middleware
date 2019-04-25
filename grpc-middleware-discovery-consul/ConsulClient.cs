using grpc_middleware_discovery;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace grpc_middleware_discovery_consul
{
    public class ConsulClient : IServiceDiscovery
    {
        private readonly HttpClient _HttpClient;

        public ConsulClient(Uri consulUri)
        {
            _HttpClient = new HttpClient
            {
                BaseAddress = consulUri
            };
        }

        public async Task<ServiceDescription> FindService(string serviceId)
        {
            var result = await _HttpClient.GetAsync($"v1/catalog/service/{serviceId}");

            if (result.IsSuccessStatusCode)
            {
                return new ServiceDescription();
            }
            return new ServiceDescription();
        }

        public async Task<bool> RegisterService(ServiceDescription serviceDescription)
        {
            var result = await _HttpClient.PutAsync("v1/catalog/register", new StringContent(JsonConvert.SerializeObject(serviceDescription)));

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
