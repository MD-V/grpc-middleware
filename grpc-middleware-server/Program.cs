using Grpc.Core;
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
        public static void Main(string[] args)
        {
            var server = new GrpcMiddlewareServer(null, "localhost")
            {
                Services = { Greeter.BindService(new GreeterImpl()) }
            };

            server.Start();
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
