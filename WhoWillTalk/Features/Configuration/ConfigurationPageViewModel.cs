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

    private bool _isLoadingProjects;
    public bool IsLoadingProjects {
        get => _isLoadingProjects;
        set => SetProperty(ref _isLoadingProjects, value);
    }

    private bool _isLoadingPeoples;
    public bool IsLoadingPeoples {
        get => _isLoadingPeoples;
        set => SetProperty(ref _isLoadingPeoples, value);
    }

    private ObservableCollection<ConfigurationAtlassianPersonViewModel> _peoples;
    public ObservableCollection<ConfigurationAtlassianPersonViewModel> Peoples {
        get => _peoples;
        set => SetProperty(ref _peoples, value);
    }

    private ObservableCollection<AtlassianProjectModel> _projects;
    public ObservableCollection<AtlassianProjectModel> Projects {
        get => _projects;
        set => SetProperty(ref _projects, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand ProjectSelectedCommand { get; }

    private AtlassianConfigurationModel _atlassianConfigurationModel;
    private CancellationTokenSource _fetchToken;

    public ConfigurationPageViewModel() {
        _atlassianConfigurationModel = AtlassianService.GetConfiguration() ?? new AtlassianConfigurationModel();
        Cookie = _atlassianConfigurationModel.Cookie;

        SaveCommand = new Command(() => {
            _atlassianConfigurationModel.Id = Guid.NewGuid().ToString();

            try {
                if (string.IsNullOrEmpty(_atlassianConfigurationModel.BoardId)) {
                    throw new Exception("Board is required");
                }

                List<AtlassianPersonDTO> selectedPersons = Peoples?.Where(p => p.Checked).Select(p => p.Person).ToList();
                if (selectedPersons is null || selectedPersons.Count == 0) {
                    throw new Exception("Select at least one person");
                }

                AtlassianService.SaveConfiguration(_atlassianConfigurationModel);
                AtlassianService.CachePersonList(selectedPersons);

                App.Current.MainPage.DisplayAlert("Sucesso", "Configuração salva", "Ok");
            } catch (Exception e) {
                App.Current.MainPage.DisplayAlert("Error", e.Message, "Ok");
            }
        });

        ProjectSelectedCommand = new Command<string>(async boardId => {
            _atlassianConfigurationModel.BoardId = boardId;

            await FetchPeople();
        });

        MainThread.InvokeOnMainThreadAsync(async () => {
            if (_atlassianConfigurationModel.Cookie is not null) {
                FetchProjects();
                await FetchPeople();
            }
        });
    }

    protected override void OnPropertyChanged(string propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(Cookie) && Cookie != _atlassianConfigurationModel.Cookie) {
            Task.Run(async () => {
                _fetchToken?.Cancel();
                _fetchToken = new CancellationTokenSource();

                await Task.Delay(500);
                if (_fetchToken.IsCancellationRequested) return;

                await MainThread.InvokeOnMainThreadAsync(async () => {
                    _atlassianConfigurationModel.Cookie = Cookie;
                    FetchProjects();
                });
            });
        }
    }

    private async void FetchProjects() {
        try {
            IsLoadingProjects = true;
            Projects = null;
            Peoples = null;

            List<AtlassianProjectDTO> projects = await AtlassianService.ListProjects(_atlassianConfigurationModel);
            if (projects is null) {
                IsLoadingProjects = false;
                return;
            }

            Projects = new ObservableCollection<AtlassianProjectModel>(projects.Select(atlassianProjectDto => new AtlassianProjectModel {
                BoardId = atlassianProjectDto.Attributes.BoardId,
                Name = atlassianProjectDto.Title
            }));

            IsLoadingProjects = false;
        } catch (Exception e) {
            // Log error
        }
    }

    private async Task FetchPeople() {
        IsLoadingPeoples = true;
        List<AtlassianPersonDTO> persons = await AtlassianService.ListPersons(_atlassianConfigurationModel);
        IsLoadingPeoples = false;
        if (persons is null) return;

        List<AtlassianPersonDTO> cachedPeople = AtlassianService.ListCachedPersons();
        Peoples = new ObservableCollection<ConfigurationAtlassianPersonViewModel>(persons.Where(p => p.Active).Select(person => new ConfigurationAtlassianPersonViewModel(person) {
            Checked = cachedPeople?.Any(p => p.Id == person.Id) ?? false
        }));
    }
}