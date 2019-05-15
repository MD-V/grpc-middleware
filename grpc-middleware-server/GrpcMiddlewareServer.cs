using Grpc.Core;
using Grpc.Health.V1;
using grpc_middleware_discovery;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace grpc_middleware_server
{
    public class GrpcMiddlewareServer
    {
        private readonly IServiceDiscovery _ServiceDiscovery;
        private readonly IEnumerable<ChannelOption> _Options;
        private readonly Policy _PortRetryPolicy;
        private readonly string _Host;
        private readonly ServerCredentials _ServerCredentials;
        private Server _ServerInstance;
        private IList<string> _ServerIds = new List<string>();

        public ICollection<ServerServiceDefinition> Services { get; }

        public ICollection<ServerPort> Ports { get; }

        public bool ServerRunning { get; private set; }

        public GrpcMiddlewareServer(IServiceDiscovery serviceDiscovery, string host, ServerCredentials serverCredentials = null, IEnumerable<ChannelOption> options = null, Policy portRetryPolicy = null)
        {
            _ServiceDiscovery = serviceDiscovery;
            _Options = options;
            _Host = host;
            Services = new List<ServerServiceDefinition>();
            Ports = new List<ServerPort>();

            if (serverCredentials == null)
            {
                _ServerCredentials = ServerCredentials.Insecure;
            }
            else
            {
                _ServerCredentials = serverCredentials;
            }

            if (portRetryPolicy == null)
            {
                _PortRetryPolicy = Policy
               .Handle<SocketException>()
               .Retry(4);
            }
        }

        public async Task Start()
        {
            _PortRetryPolicy.Execute(() =>
            {

                if (!Ports.Any())
                {
                    var port = FreeTcpPort();
                    Ports.Add(new ServerPort(_Host, port, _ServerCredentials));
                }

                if (_Options != null)
                {
                    _ServerInstance = new Server(_Options);
                }
                else
                {
                    _ServerInstance = new Server();
                }

                foreach (var port in Ports)
                {
                    _ServerInstance.Ports.Add(port);
                }

               
                foreach (var srv in Services)
                {
                    _ServerInstance.Services.Add(srv);
                }

                // Add health check service
                _ServerInstance.Services.Add(Health.BindService(new HealthCheckImpl(this)));

                _ServerInstance.Start();

#if DEBUG
                Debug.WriteLine("Server running");
                Debug.WriteLine("Endpoints:");
                Debug.WriteLine($"   {string.Join(",", _ServerInstance.Ports.Select(a => $"{a.Host}:{a.Port}"))}");
#endif
            });

            ServerRunning = true;

            foreach (var adress in _ServerInstance.Ports)
            {
                var uriBuilder = new UriBuilder();
                uriBuilder.Host = adress.Host;
                uriBuilder.Port = adress.Port;
                uriBuilder.Scheme = string.Empty;

                var adressString = uriBuilder.Uri.ToString();

                var id = $"{nameof(GrpcMiddlewareServer)}_{Guid.NewGuid().ToString()}";

                _ServerIds.Add(id);

                await _ServiceDiscovery.RegisterService(new ServiceDescription()
                {
                    Adress = adressString,
                    Id = id,
                    Port = adress.Port,
                    Name = nameof(GrpcMiddlewareServer),
                });
            }

            
        }

        public async Task ShutdownAsync()
        {
            foreach (var serverId in _ServerIds)
            {
                await _ServiceDiscovery.DeregisterService(serverId);
            }
            await _ServerInstance.ShutdownAsync();

            ServerRunning = false;
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
    }
}
