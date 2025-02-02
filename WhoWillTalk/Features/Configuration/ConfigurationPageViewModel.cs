using System.Collections.ObjectModel;
using System.Windows.Input;
using WhoWillTalk.Atlassian.Dtos;

namespace WhoWillTalk.Features.Configuration;

public class ConfigurationPageViewModel : BaseViewModel {

    private string _cookie;
    public string Cookie {
        get => _cookie;
        set => SetProperty(ref _cookie, value);
    }
    
    private string _project;
    public string Project {
        get => _project;
        set => SetProperty(ref _project, value);
    }

    private bool _isLoading;
    public bool IsLoading {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private ObservableCollection<ConfigurationAtlassianPersonViewModel> _persons;
    public ObservableCollection<ConfigurationAtlassianPersonViewModel> Persons {
        get => _persons;
        set => SetProperty(ref _persons, value);
    }

    public ICommand SaveCommand { get; }
    private readonly AtlassianConfigurationModel _atlassianConfigurationModel;
    private CancellationTokenSource _fetchToken;
    private readonly object _personLock = new();

    public ConfigurationPageViewModel() {
        _atlassianConfigurationModel = AtlassianService.GetConfiguration() ?? new AtlassianConfigurationModel();
        Cookie = _atlassianConfigurationModel.Cookie;
        Project = _atlassianConfigurationModel.Project;

        SaveCommand = new Command(() => {
            _atlassianConfigurationModel.Id = Guid.NewGuid().ToString();

            try {
                if (string.IsNullOrEmpty(_atlassianConfigurationModel.Cookie)) {
                    throw new Exception("Cookie is required");
                }
                
                if (string.IsNullOrEmpty(_atlassianConfigurationModel.Project)) {
                    throw new Exception("Project name is required");
                }                

                if (Persons is null || !Persons.Any(p => p.Checked)) {
                    throw new Exception("Select at least one person");
                }

                AtlassianService.SaveConfiguration(_atlassianConfigurationModel);
                AtlassianService.CachePersonList(Persons.Select(p => p.Person).ToList());

                App.Current.MainPage.DisplayAlert("Sucesso", "Configuração salva", "Ok");
            } catch (Exception e) {
                App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
            }
        });

        MainThread.InvokeOnMainThreadAsync(async () => {
            if (_atlassianConfigurationModel.Cookie is not null) {
                await Fetch();
            }
        });
    }

    protected override void OnPropertyChanged(string propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Cookie) && Cookie != _atlassianConfigurationModel.Cookie) {
            QueueFetch();
        }
        
        if (propertyName == nameof(Project) && Project != _atlassianConfigurationModel.Project) {
            QueueFetch();
        }
    }

    private void QueueFetch() {
        if (string.IsNullOrEmpty(Cookie) || string.IsNullOrEmpty(Project)) return;
        
        Task.Run(async () => {
            _fetchToken?.Cancel();
            _fetchToken = new CancellationTokenSource();

            await Task.Delay(500);
            if (_fetchToken.IsCancellationRequested) return;

            await MainThread.InvokeOnMainThreadAsync(async () => {
                _atlassianConfigurationModel.Cookie = Cookie;
                _atlassianConfigurationModel.Project = Project;
                await Fetch();
            });
        });
    }

    private async Task Fetch() {
        if (IsLoading) return;
        
        IsLoading = true;
        Persons = null;

        List<AtlassianBoardDTO> boards = await AtlassianService.ListBoards(_atlassianConfigurationModel);
        if (boards is null) {
            IsLoading = false;
            return;
        }

        await FetchPersons(boards);

        await Task.Run(async () => {
            await Task.Delay(500);
            MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
        });
    }

    private async Task FetchPersons(List<AtlassianBoardDTO> boards) {
        List<AtlassianPersonDTO> persons = [];
        
        List<Task> tasks = [];

        foreach (AtlassianBoardDTO boardValue in boards) {
            tasks.Add(Task.Run(async () => {
                List<AtlassianPersonDTO> boardPersons = await AtlassianService.ListPersons(
                    new AtlassianConfigurationModel
                    {
                        Cookie = _atlassianConfigurationModel.Cookie,
                        BoardId = boardValue.Id.ToString()
                    });

                foreach (AtlassianPersonDTO atlassianPersonDto in boardPersons) {
                    if (persons.Any(p => p.Id == atlassianPersonDto.Id)) continue;

                    lock (_personLock) {
                        persons.Add(atlassianPersonDto);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        if (persons is null) return;

        List<AtlassianPersonDTO> cachedPersons = AtlassianService.ListCachedPersons();
        Persons = new ObservableCollection<ConfigurationAtlassianPersonViewModel>(persons.Where(p => p.Active).Select(person => {
            AtlassianPersonDTO cachedPerson = cachedPersons?.FirstOrDefault(p => p.Id == person.Id);

            if (cachedPerson is not null) {
                person.Nickname = cachedPerson.Nickname;
                person.IsChecked = cachedPerson.IsChecked;
            }

            return new ConfigurationAtlassianPersonViewModel(person);
        }));
    }
}