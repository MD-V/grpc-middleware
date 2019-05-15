using System.Collections.Generic;
using System.Threading.Tasks;

namespace grpc_middleware_discovery
{
    public interface IServiceDiscovery
    {
        Task<bool> RegisterService(ServiceDescription serviceDescription);

        Task<ServiceDescription> FindServiceLocal(string serviceId);

        Task<IEnumerable<ServiceDescription>> FindService(string serviceName);

        Task<bool> DeregisterService(string id);
    }
}
