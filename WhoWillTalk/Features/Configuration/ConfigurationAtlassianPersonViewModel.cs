using System.Windows.Input;

namespace WhoWillTalk.Features.Configuration;

public class ConfigurationAtlassianPersonViewModel : BaseViewModel {

    public AtlassianPersonDTO Person { get; set; }
    public string Name { get; set; }
    public bool Checked { get; set; }

    public ICommand TapCommand { get; set; }

    public ConfigurationAtlassianPersonViewModel(AtlassianPersonDTO person) {
        Person = person;
        Name = person.Name;

        TapCommand = new Command(() => {
            Checked = !Checked;
        });
    }

}