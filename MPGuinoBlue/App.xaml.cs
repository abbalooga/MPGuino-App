using MPGuinoBlue.Views;
using Xamarin.Forms;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MPGuinoBlue
{
    

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

           

        MainPage = new NavigationPage(new MainPage())
        {
            BarBackgroundColor = Color.Black,
            BarTextColor = Color.White
        };
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
