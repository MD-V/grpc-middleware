using Grpc.Core;
using grpc_middleware_discovery_consul;
using Helloworld;
using System;
using System.Threading.Tasks;

namespace grpc_middleware_server
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }

    class Program
    {
        public async static Task Main(string[] args)
        {
            var server = new GrpcMiddlewareServer(new ConsulClient(new Uri("http://127.0.0.1:8500")), "localhost")
            {
                Services = { Greeter.BindService(new GreeterImpl()) }
            };

            await server.Start();
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
