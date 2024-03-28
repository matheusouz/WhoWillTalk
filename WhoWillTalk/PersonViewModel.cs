namespace WhoWillTalk;

public class PersonViewModel : BaseViewModel {

    private ImageSource _source;
    public ImageSource Source {
        get => _source;
        set => SetProperty(ref _source, value);
    }

    private string _name;
    public string Name {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _talked;
    public bool Talked {
        get => _talked;
        set => SetProperty(ref _talked, value);
    }

    private double _opacity = 1;
    public double Opacity {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    protected override void OnPropertyChanged(string propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if (propertyName is nameof(Talked)) {
            Opacity = Talked ? 0.54 : 1;
        }
    }
}