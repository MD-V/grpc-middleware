using Grpc.Core;
using grpc_middleware_discovery_consul;
using Helloworld;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace grpc_middleware_client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var discovery = new ConsulClient(new Uri("http://127.0.0.1:8500"));

           var availableServices = await discovery.FindService("GrpcMiddleWareServer");

            var firstService = availableServices.FirstOrDefault();

            if(firstService != null)
            {
                Channel channel = new Channel(firstService.Adress, ChannelCredentials.Insecure);

                var policy = Policy
                    .Handle<RpcException>()
                    .WaitAndRetry(5, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                );

                var client = new Greeter.GreeterClient(new BasicCallInvoker(channel, policy));
                string user = "you";
                var reply = client.SayHello(new HelloRequest { Name = user }, new CallOptions().WithWaitForReady(true));
                Console.WriteLine("Greeting: " + reply.Message);

                channel.ShutdownAsync().Wait();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Could not find service");
            }

            
        }
    }
}
