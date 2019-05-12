using Newtonsoft.Json;

namespace grpc_middleware_discovery_consul.Agent
{
    // <summary>
    /// AgentCheck represents a check known to the agent
    /// </summary>
    public class AgentCheck
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ID { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Interval { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Notes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string DeregisterCriticalServiceAfter { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GRPC { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Timeout { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TTL { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceID { get; set; }      
    }
}
