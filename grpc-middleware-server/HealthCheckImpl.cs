using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;

namespace grpc_middleware_server
{
    internal class HealthCheckImpl : Health.HealthBase
    {
        private readonly GrpcMiddlewareServer _ServerInstance;

        public HealthCheckImpl(GrpcMiddlewareServer serverInstance)
        {
            _ServerInstance = serverInstance;
        }

        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context)
        {
            if (_ServerInstance.ServerRunning)
            {
                return Task.FromResult(new HealthCheckResponse()
                {
                    Status = HealthCheckResponse.Types.ServingStatus.Serving
                });
            }
            else
            {
                return Task.FromResult(new HealthCheckResponse()
                {
                    Status = HealthCheckResponse.Types.ServingStatus.NotServing
                });
            }
        }
    }
}
