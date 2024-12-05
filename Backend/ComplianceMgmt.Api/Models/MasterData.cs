using Newtonsoft.Json;

namespace ComplianceMgmt.Api.Models
{
    public class MasterData
    {
        [JsonProperty("Master Name")]
        public string MasterName { get; set; }
        
        [JsonProperty("Master Values Desc")]
        public string MasterValuesDesc { get; set; }
        
        [JsonProperty("Code")]
        public string Code { get; set; }
    }
}
