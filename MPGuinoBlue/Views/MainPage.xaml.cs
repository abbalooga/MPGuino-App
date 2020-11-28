using MPGuinoBlue.ViewModels;
using mpg.Helpers;
using Xamarin.Forms;

namespace MPGuinoBlue.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new MainPageViewModel();
            xamlSwitch.Toggled += saveswitch;
            xamlSwitchScan.Toggled += scanswitch;

        }


        void scanswitch(object sender, ToggledEventArgs e)
        {
            Settings.savescanswitch(e);

        }
        void saveswitch(object sender, ToggledEventArgs e)
        {
            Settings.savesaveswitch(e);
            
                }
        
    }
}
