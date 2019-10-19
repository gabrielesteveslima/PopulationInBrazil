using Newtonsoft.Json;

namespace PopulationWorkerService.Projections
{
    public class IbgeResponse
    {
        [JsonProperty("projecao")] public ProjectionInBrazil ProjectionInBrazil { get; set; }
    }
}