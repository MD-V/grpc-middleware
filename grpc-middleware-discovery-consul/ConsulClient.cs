using grpc_middleware_discovery;
using grpc_middleware_discovery_consul.Agent;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace grpc_middleware_discovery_consul
{
    public class ConsulClient : IServiceDiscovery
    {
        private readonly HttpClient _HttpClient;

        public ConsulClient(Uri consulUri)
        {
            _HttpClient = new HttpClient
            {
                BaseAddress = consulUri
            };
        }

        /// <summary>
        /// Finds a service by its id. Uses the catalog
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        public async Task<ServiceDescription> FindServiceLocal(string serviceId)
        {
            var result = await _HttpClient.GetAsync($"v1/agent/service/{serviceId}");

            if (result.IsSuccessStatusCode)
            {
                var returnedService = await result.Content.ReadAsStringAsync();

                var serviceRegistration = JsonConvert.DeserializeObject<AgentServiceRegistration>(returnedService);

                return new ServiceDescription()
                {
                    Port = serviceRegistration.Port,
                    Adress = serviceRegistration.Address,
                    Id = serviceRegistration.ID,
                    Name = serviceRegistration.Name,
                };
            }
            return null;
        }


        public async Task<IEnumerable<ServiceDescription>> FindService(string serviceName)
        {
            var result = await _HttpClient.GetAsync($"v1/catalog/service/{serviceName}");

            if (result.IsSuccessStatusCode)
            {
                var returnedService = await result.Content.ReadAsStringAsync();

                var serviceRegistrations = JsonConvert.DeserializeObject<IEnumerable<CatalogServiceRegistration>>(returnedService);
                return serviceRegistrations.Select(a => new ServiceDescription()
                {
                    Port = a.ServicePort,
                    Adress = a.ServiceAddress,
                    Id = a.ServiceID,
                    Name = a.ServiceName,
                });
            }
            return null;
        }

        /// <summary>
        /// Registers the service on a remote machine using the catalog api
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        public async Task<bool> RegisterServiceRemote(ServiceDescription serviceDescription)
        {
            var result = await _HttpClient.PutAsync("v1/catalog/register", new StringContent(JsonConvert.SerializeObject(serviceDescription)));

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers the service on the local consul node using the agent api
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        public async Task<bool> RegisterService(ServiceDescription serviceDescription)
        {
            var agentServiceRegistration = new AgentServiceRegistration()
            {
                Address = serviceDescription.Adress,
                ID = serviceDescription.Id,
                Name = serviceDescription.Name,
                Port = serviceDescription.Port,
                Check = new AgentCheck()
                {
                    GRPC = serviceDescription.Adress,
                    Interval = "30s",
                    Name = $"{serviceDescription.Name}_GRPCHealth",
                    ID = $"{serviceDescription.Id}_GRPCHealth",
                    Timeout = "10s",
                    DeregisterCriticalServiceAfter = "2m"
                }
            };

            var result = await _HttpClient.PutAsync("v1/agent/service/register", new StringContent(JsonConvert.SerializeObject(agentServiceRegistration)));

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers the service on the local consul node using the agent api
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <returns></returns>
        public async Task<bool> DeregisterService(string id)
        {
            var result = await _HttpClient.PutAsync($"v1/agent/service/deregister/{id}", null);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        private ConcurrentDictionary<string, ConcurrentBag<IServiceObserver>> _ServiceObservers = new ConcurrentDictionary<string, ConcurrentBag<IServiceObserver>>();
        private ConcurrentDictionary<string, ConcurrentBag<ServiceDescription>> _Services = new ConcurrentDictionary<string, ConcurrentBag<ServiceDescription>>();
        private ConcurrentBag<Task> _ServiceObserverTasks = new ConcurrentBag<Task>();
        private Policy _ServiceObserverPolicy = Policy.Bulkhead(8);

        public void SubscribeService(IServiceObserver serviceObserver)
        {
            Console.WriteLine("SubscribeService");
            var observerBag = _ServiceObservers.GetOrAdd(serviceObserver.ServiceName, s => new ConcurrentBag<IServiceObserver>());
            observerBag.Add(serviceObserver);

            var servicesBag = _Services.GetOrAdd(serviceObserver.ServiceName, s => new ConcurrentBag<ServiceDescription>());
            if (!servicesBag.IsEmpty)
            {
                Console.WriteLine("SubscribeService servicesBag is not empty");
                serviceObserver.ServicesChanged?.Invoke(new ServicesChanged(servicesBag.Select(x => new ChangedService(x, ServiceChangeType.Created))));
            }
            else
            {
                Console.WriteLine("SubscribeService servicesBag is empty");
                _ServiceObserverTasks.Add(Task.Run(async () =>
                {
                    while (true)
                    { Console.WriteLine("SubscribeService Find Services " + serviceObserver.ServiceName);
                       
                        var services = await FindService(serviceObserver.ServiceName);
                        var newServices = services.Where(s => !servicesBag.Select(x => x.Id).Contains(s.Id)).ToArray();
                        var oldServices = servicesBag.Where(s => !services.Select(x => x.Id).Contains(s.Id)).ToArray();
                        while (!servicesBag.IsEmpty)
                        {
                            servicesBag.TryTake(out ServiceDescription s);
                        }
                        foreach (var currentService in services)
                        {
                            servicesBag.Add(currentService);
                        }

                        var serviceChanges = new List<ChangedService>();
                        serviceChanges.AddRange(oldServices.Select(os => new ChangedService(os, ServiceChangeType.Closed)));
                        serviceChanges.AddRange(newServices.Select(os => new ChangedService(os, ServiceChangeType.Created)));

                        if (serviceChanges.Any())
                        {
                            foreach (var observer in observerBag)
                            {
                                observer.ServicesChanged?.Invoke(new ServicesChanged(serviceChanges));
                            }
                        }

                        Thread.Sleep(10 * 1000);
                    }
                }));
            }
        }
    }

}
