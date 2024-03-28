using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoWillTalk.Features.Configuration;

public partial class ConfigurationPage : ContentPage {
    public ConfigurationPage() {
        InitializeComponent();

        BindingContext = new ConfigurationPageViewModel();
    }
}