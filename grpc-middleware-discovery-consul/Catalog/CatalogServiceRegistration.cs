using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace grpc_middleware_discovery_consul.Agent
{
    /// <summary>
    /// AgentServiceRegistration is used to register a new service
    /// </summary>
    public class CatalogServiceRegistration
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] ServiceTags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ServicePort { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceAddress { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool ServiceEnableTagOverride { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> ServiceMeta { get; set; }
    }
}
