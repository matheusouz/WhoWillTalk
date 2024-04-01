using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WhoWillTalk.Atlassian.Dtos;
using WhoWillTalk.Features.Configuration;

namespace WhoWillTalk.Atlassian.Request;

public class AtlassianRequestManager {

    public static async Task<List<AtlassianPersonDTO>> ListPersons(AtlassianConfigurationModel configuration) {
        string content = await DoPost(configuration, $"https://asaasdev.atlassian.net/rest/boards/latest/board/{configuration.BoardId}?hideCardExtraFields=true&includeHidden=true&moduleKey=agile-mobile-board-service&onlyUseEpicsFromIssues=true&skipEtag=true&skipExtraFields=true");
        if (content is null) return null;

        List<AtlassianPersonDTO> persons = new List<AtlassianPersonDTO>();
        JObject jObject = JsonConvert.DeserializeObject<JObject>(content);

        JToken person = jObject["people"];

        foreach (JProperty jProperty in person.ToList()) {
            persons.Add(new AtlassianPersonDTO {
                Id = jProperty.Value["userKey"].ToString(),
                Name = jProperty.Value["displayName"].ToString(),
                AvatarUrl = jProperty.Value["avatarUrl"].ToString(),
                Active = jProperty.Value["active"].ToObject<bool>()
            });
        }

        return persons;
    }


    public static async Task<List<AtlassianProjectDTO>> ListProjects(AtlassianConfigurationModel configuration) {
        string content = await DoPost(configuration , "https://asaasdev.atlassian.net/rest/internal/2/productsearch/search?counts=projects%3D5&type=projects");
        if (content is "" or null) return null;

        List<AtlassianRecentProjectsDTO> recentProjects = JsonConvert.DeserializeObject<List<AtlassianRecentProjectsDTO>>(content);

        return recentProjects.First().Items.Select(dto => dto.Project).ToList();
    }

    private static async Task<string> DoPost(AtlassianConfigurationModel configuration, string url) {
        if (configuration.Cookie == "") return null;

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("sec-ch-ua", "\"Brave\";v=\"123\", \"Not:A-Brand\";v=\"8\", \"Chromium\";v=\"123\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "\"macOS\"");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
        request.Headers.Add("Sec-GPC", "1");
        request.Headers.Add("host", "asaasdev.atlassian.net");
        request.Headers.Add("Cookie",  $"tenant.session.token={configuration.Cookie}");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}