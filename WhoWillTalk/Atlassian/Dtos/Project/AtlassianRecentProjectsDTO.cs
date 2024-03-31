using Newtonsoft.Json;

namespace WhoWillTalk.Atlassian.Dtos;

public class AtlassianRecentProjectsDTO {

    [JsonProperty("itemsWithLastAccessed")]
    public List<AtlassianProjectInfoDTO> Items { get; set; }
}