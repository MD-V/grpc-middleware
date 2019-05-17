using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace grpc_middleware_discovery
{
    public interface IServiceDiscovery
    {
        Task<bool> RegisterService(ServiceDescription serviceDescription);
        Task<bool> DeregisterService(string id);


        Task<ServiceDescription> FindServiceLocal(string serviceId);

        Task<IEnumerable<ServiceDescription>> FindService(string serviceName);

        void SubscribeService(IServiceObserver serviceObserver);

    }

    /// <summary>
    /// Interface used to observe a specific service.
    /// Get Notifications when Service is closed or a new ServiceInstance is available
    /// 
    /// To Unsubscribe call Dispose on observer Object
    /// </summary>
    public interface IServiceObserver : IDisposable
    {
        string ServiceName { get; }

        Action<ServicesChanged> ServicesChanged { get; }
    }

    public class ServiceObserver : IServiceObserver
    {

        public ServiceObserver(string serviceName, Action<ServicesChanged> servicesChangedCallback)
        {
            ServiceName = serviceName;
            ServicesChanged = servicesChangedCallback;
        }

        public string ServiceName { get; }

        public Action<ServicesChanged> ServicesChanged { get; }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }

    public class ServicesChanged
    {
        public ServicesChanged(IEnumerable<ChangedService> changedServices)
        {
            ChangedServices = changedServices;
        }

        public IEnumerable<ChangedService> ChangedServices { get; }
    }

    public class ChangedService
    {
        public ChangedService(ServiceDescription serviceDescription, ServiceChangeType changeType)
        {
            ServiceDescription = serviceDescription;
            ChangeType = changeType;
        }

       public ServiceDescription ServiceDescription { get; }
        public ServiceChangeType ChangeType { get; }
    }

    public enum ServiceChangeType
    {
        Created,
        Closed
    }
}
