using System.Collections.ObjectModel;
using System.Windows.Input;

namespace WhoWillTalk;

public class MainPageViewModel : BaseViewModel {

    private ObservableCollection<PersonViewModel> _persons;
    public ObservableCollection<PersonViewModel> Persons {
        get => _persons;
        set => SetProperty(ref _persons, value);
    }

    private PersonViewModel _personTalking;
    public PersonViewModel PersonTalking {
        get => _personTalking;
        set => SetProperty(ref _personTalking, value);
    }

    private bool _hasPersonTalking;
    public bool HasPersonTalking {
        get => _hasPersonTalking;
        set => SetProperty(ref _hasPersonTalking, value);
    }

    public ICommand NextCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand RemoveCommand { get; }

    public MainPageViewModel() {
        ResetCommand = new Command(ResetPersons);
        NextCommand = new Command(() => {
            if (PersonTalking is not null) {
                PersonTalking.Talked = true;
            }

            List<PersonViewModel> waitingToTalk = Persons.Where(p => !p.Talked).ToList();
            PersonTalking = waitingToTalk.OrderBy(a => Guid.NewGuid()).ToList().FirstOrDefault();
        });

        RemoveCommand = new Command<PersonViewModel>(model => {
            Persons.Remove(model);
        });

        ResetPersons();
    }

    protected override void OnPropertyChanged(string propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if (propertyName is nameof(PersonTalking)) {
            HasPersonTalking = PersonTalking is not null;
        }

    }

    private void ResetPersons() {
        List<PersonViewModel> personViewModels = new List<PersonViewModel> {
            new PersonViewModel { Name = "Person 1" },
            new PersonViewModel { Name = "Person 2" },
            new PersonViewModel { Name = "Person 3" },
            new PersonViewModel { Name = "Person 4" },
            new PersonViewModel { Name = "Person 5" },
            new PersonViewModel { Name = "Person 6" },
            new PersonViewModel { Name = "Person 7" },
            new PersonViewModel { Name = "Person 8" },
            new PersonViewModel { Name = "Person 9" },
            new PersonViewModel { Name = "Person 10" },
            new PersonViewModel { Name = "Person 11" },
            new PersonViewModel { Name = "Person 12" },
            new PersonViewModel { Name = "Person 13" },
            new PersonViewModel { Name = "Person 14" },
            new PersonViewModel { Name = "Person 15" },
            new PersonViewModel { Name = "Person 16" },
            new PersonViewModel { Name = "Person 17" },
            new PersonViewModel { Name = "Person 18" },
            new PersonViewModel { Name = "Person 19" },
            new PersonViewModel { Name = "Person 20" },
        };

        Persons = new ObservableCollection<PersonViewModel>(personViewModels.OrderBy(p => p.Name).ToList());
        PersonTalking = null;
    }
}