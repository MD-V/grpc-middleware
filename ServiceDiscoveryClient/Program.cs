// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Grpc.Core;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            // Server
            var server = new Servicediscovery.ServiceRegistration.ServiceRegistrationClient(channel);

            var registerReply = server.Register(new Servicediscovery.RegisterRequest()
            {
                Entry = new Servicediscovery.ServiceEntry()
                {
                    Endpoint = "localhost:1337",
                    Hostname = "MICHI-PC",
                    Instance = "1",
                    Name = "TestServer",
                    
                }
            });

            registerReply = server.Register(new Servicediscovery.RegisterRequest()
            {
                Entry = new Servicediscovery.ServiceEntry()
                {
                    Endpoint = "localhost:0815",
                    Hostname = "MICHI-PC",
                    Instance = "2",
                    Name = "TestServer",

                }
            });

            registerReply = server.Register(new Servicediscovery.RegisterRequest()
            {
                Entry = new Servicediscovery.ServiceEntry()
                {
                    Endpoint = "localhost:5711",
                    Hostname = "MICHI-PC",
                    Instance = "3",
                    Name = "TestServer",

                }
            });

            // Client
            var client = new Servicediscovery.ServiceCatalog.ServiceCatalogClient(channel);

            var reply = client.ListServices(new Servicediscovery.ListServicesRequest()
            {
                Name = "TestServer"
            });
                
            foreach(var entry in reply.Entries)
            {
                Console.WriteLine("Service: " + entry.Endpoint);
            }

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
