using Newtonsoft.Json;

namespace PopulationWorkerService.Projections
{
    public class ProjectionInBrazil
    {
        [JsonProperty("populacao")] public int Population { get; set; }
    }
}