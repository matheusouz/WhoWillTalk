using System.Windows.Input;

namespace WhoWillTalk.Features.Configuration;

public class ConfigurationAtlassianPersonViewModel : BaseViewModel {

    public AtlassianPersonDTO Person { get; set; }
    public string Name { get; set; }

    private bool _checked;
    public bool Checked {
        get => _checked;
        set => SetProperty(ref _checked, value);
    }

    private string _nickname;
    public string Nickname {
        get => _nickname;
        set => SetProperty(ref _nickname, value);
    }

    public ICommand TapCommand { get; set; }

    public ICommand EditCommand { get; set; }

    private bool isEditing;

    public ConfigurationAtlassianPersonViewModel(AtlassianPersonDTO person) {
        Person = person;
        Name = person.Name;
        Nickname = person.Nickname;
        Checked = person.IsChecked;

        TapCommand = new Command(() => {
            Checked = !Checked;
            Person.IsChecked = Checked;
        });

        EditCommand = new Command(async () => {
            if (isEditing) return;

            isEditing = true;
            string nickname = await App.Current.MainPage.DisplayPromptAsync("Apelido", "Digite o apelido");
            Nickname = person.Nickname = nickname;
            isEditing = false;
        });
    }

}