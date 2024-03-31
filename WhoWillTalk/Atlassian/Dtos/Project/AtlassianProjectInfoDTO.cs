using Newtonsoft.Json;

namespace WhoWillTalk.Atlassian.Dtos;

public class AtlassianProjectInfoDTO {

    [JsonProperty("resultItem")]
    public AtlassianProjectDTO Project { get; set; }
}