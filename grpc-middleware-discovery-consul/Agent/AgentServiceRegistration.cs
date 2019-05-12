using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace grpc_middleware_discovery_consul.Agent
{
    /// <summary>
    /// AgentServiceRegistration is used to register a new service
    /// </summary>
    public class AgentServiceRegistration
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Port { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool EnableTagOverride { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AgentCheck Check { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AgentCheck[] Checks { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Meta { get; set; }
    }
}
