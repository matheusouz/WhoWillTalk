using WhoWillTalk.Features.Configuration;

namespace WhoWillTalk;

public partial class MainPage {
    public MainPage() {
        InitializeComponent();
        BindingContext = new MainPageViewModel();
    }

    private void ImageButton_OnClicked(object sender, EventArgs e) {
        Application.Current.MainPage.Navigation.PushAsync(new ConfigurationPage());
    }
}