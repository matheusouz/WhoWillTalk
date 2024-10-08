using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Converters;
using WhoWillTalk.Features.Configuration;

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

    private bool _canDisplayOptions;
    public bool CanDisplayOptions {
        get => _canDisplayOptions;
        set => SetProperty(ref _canDisplayOptions, value);
    }

    private bool _isLoading;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private bool _isNotConfigured;
    public bool IsNotConfigured {
        get => _isNotConfigured;
        set => SetProperty(ref _isNotConfigured, value);
    }

    private bool _isSpeechEnabled;
    public bool IsSpeechEnabled {
        get => _isSpeechEnabled;
        set => SetProperty(ref _isSpeechEnabled, value);
    }

    public ICommand NextCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand RemoveCommand { get; }

    private List<AtlassianPersonDTO> _personList;
    private AtlassianConfigurationModel _atlassianConfigurationModel;

    public MainPageViewModel() {
        IsSpeechEnabled = AtlassianService.IsSpeechEnabled();

        ResetCommand = new Command(ResetPersons);
        NextCommand = new Command(async () => {
            if (PersonTalking is not null) {
                PersonTalking.Talked = true;
            }

            List<PersonViewModel> waitingToTalk = Persons.Where(p => !p.Talked).ToList();
            PersonTalking = waitingToTalk.OrderBy(a => Guid.NewGuid()).ToList().FirstOrDefault();

            if (PersonTalking is not null && IsSpeechEnabled) {
                IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();

                locales = locales.Where(locale => locale.Language == "pt-BR");

                SpeechOptions options = new SpeechOptions {
                    Pitch = 1.5f,   // 0.0 - 2.0
                    Volume = 0.75f, // 0.0 - 1.0
                    Locale = locales.FirstOrDefault()
                };

                await TextToSpeech.SpeakAsync(PersonTalking.Name, options);
            }
        });

        SkipCommand = new Command(() => {
            List<PersonViewModel> waitingToTalk = Persons.Where(p => !p.Talked).ToList();
            PersonTalking = waitingToTalk.OrderBy(a => Guid.NewGuid()).ToList().FirstOrDefault();
        });

        RemoveCommand = new Command<PersonViewModel>(model => {
            Persons.Remove(model);
        });

        UpdatePersons();
    }

    private void UpdatePersons() {
        _atlassianConfigurationModel = AtlassianService.GetConfiguration();
        if (_atlassianConfigurationModel is null) {
            IsNotConfigured = true;
            return;
        }

        IsNotConfigured = false;

        ResetPersons();
    }

    protected override void OnPropertyChanged(string propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if (propertyName is nameof(PersonTalking)) {
            HasPersonTalking = PersonTalking is not null;
        }

        if (propertyName is nameof(IsSpeechEnabled)) {
            AtlassianService.SaveSpeech(IsSpeechEnabled);
        }
    }

    private void ResetPersons() {
        _personList = AtlassianService.ListCachedPersons();

        if (_personList is not null) {
            List<PersonViewModel> personViewModels = _personList.Where(person => person.Active && person.IsChecked).Select(person =>
                new PersonViewModel {
                    Name = person.Nickname ?? person.Name,
                    Source = person.AvatarUrl,
                    Talked = false
                }).ToList();

            Persons = new ObservableCollection<PersonViewModel>(personViewModels.OrderBy(p => p.Name).ToList());
            PersonTalking = null;

            CanDisplayOptions = true;
        }
    }

    public void OnAppearing() {
        AtlassianConfigurationModel atlassianConfigurationModel = AtlassianService.GetConfiguration();
        if (atlassianConfigurationModel is null) return;

        if (_atlassianConfigurationModel is null) {
            _atlassianConfigurationModel = atlassianConfigurationModel;
            UpdatePersons();
        }

        if (atlassianConfigurationModel.Id != _atlassianConfigurationModel.Id) {
            _atlassianConfigurationModel = atlassianConfigurationModel;
            UpdatePersons();
        }
    }
}