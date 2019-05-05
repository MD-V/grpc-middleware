using Grpc.Core;
using Helloworld;
using Polly;
using System;

namespace grpc_middleware_client
{
    class Program
    {
        static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var policy = Policy
                .Handle<RpcException>()
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
            );

            var client = new Greeter.GreeterClient(new BasicCallInvoker(channel, policy));
            string user = "you";

            Console.ReadLine();

            

            var reply = client.SayHello(new HelloRequest { Name = user }, new CallOptions().WithWaitForReady(true));
            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
