using Newtonsoft.Json;
using WhoWillTalk.Atlassian.Dtos;
using WhoWillTalk.Atlassian.Request;
using WhoWillTalk.Features.Configuration;

namespace WhoWillTalk;

public class AtlassianService {

    public static async Task<List<AtlassianPersonDTO>> ListPersons(AtlassianConfigurationModel configuration) {
        return await AtlassianRequestManager.ListPersons(configuration);
    }

    public static async Task<List<AtlassianProjectDTO>> ListProjects(AtlassianConfigurationModel configuration) {
        return await AtlassianRequestManager.ListProjects(configuration);
    }

    public static void CachePersonList(List<AtlassianPersonDTO> persons) {
        string json = JsonConvert.SerializeObject(persons);
        Preferences.Set("persons", json);
    }

    public static List<AtlassianPersonDTO> ListCachedPersons() {
        string json = Preferences.Get("persons", string.Empty);
        if (string.IsNullOrEmpty(json)) return null;

        return JsonConvert.DeserializeObject<List<AtlassianPersonDTO>>(json);
    }

    public static AtlassianConfigurationModel GetConfiguration() {
        string json = Preferences.Get("new-configuration", string.Empty);
        if (string.IsNullOrEmpty(json)) return null;

        return JsonConvert.DeserializeObject<AtlassianConfigurationModel>(json);
    }

    public static void SaveConfiguration(AtlassianConfigurationModel atlassianConfigurationModel) {
        string json = JsonConvert.SerializeObject(atlassianConfigurationModel);
        Preferences.Set("new-configuration", json);
    }

    public static void SaveSpeech(bool enabled) {
        Preferences.Set("speech-enabled", enabled);
    }

    public static bool IsSpeechEnabled() {
        return Preferences.Get("speech-enabled", false);
    }
}