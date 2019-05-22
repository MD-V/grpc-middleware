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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Servicediscovery;

namespace GreeterServer
{
    class ServiceRegistrationImpl : ServiceRegistration.ServiceRegistrationBase
    {
        private object _RegisterLock = new object();

        private List<ServiceEntry> _Entries = new List<ServiceEntry>();

        public ServiceRegistrationImpl(object lockobj, List<ServiceEntry> entries)
        {
            _Entries = entries;
            _RegisterLock = lockobj;
        }

        public async override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            lock (_Entries)
            {
                _Entries.Add(request.Entry);
            }

            return new RegisterReply()
            {
                Success = true
            };
        }
    }

    class ServiceCatalogImpl : ServiceCatalog.ServiceCatalogBase
    {
        private  readonly object _RegisterLock;

        private readonly List<ServiceEntry> _Entries;

        public ServiceCatalogImpl(object lockobj, List<ServiceEntry> entries)
        {
            _Entries = entries;
            _RegisterLock = lockobj;

        }

        public async override Task<ListServicesReply> ListServices(ListServicesRequest request, ServerCallContext context)
        {
            List<ServiceEntry> localEntries;
            lock (_Entries)
            {
                localEntries = _Entries.ToList();
            }

            var reply = new ListServicesReply();

            reply.Entries.AddRange(localEntries);


            return reply;
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            object registerLock = new object();

            List<ServiceEntry> entries = new List<ServiceEntry>();


            Server server = new Server
            {
                Services = { ServiceRegistration.BindService(new ServiceRegistrationImpl(registerLock, entries)), ServiceCatalog.BindService(new ServiceCatalogImpl(registerLock, entries)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
