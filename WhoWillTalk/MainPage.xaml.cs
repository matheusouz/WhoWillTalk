namespace WhoWillTalk;

public partial class MainPage {
    public MainPage() {
        InitializeComponent();
        BindingContext = new MainPageViewModel();
    }
}