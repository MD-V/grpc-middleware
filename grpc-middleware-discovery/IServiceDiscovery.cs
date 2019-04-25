using System.Threading.Tasks;

namespace grpc_middleware_discovery
{
    public interface IServiceDiscovery
    {
        Task<bool> RegisterService(ServiceDescription serviceDescription);

        Task<ServiceDescription> FindService(string serviceId);
    }
}
