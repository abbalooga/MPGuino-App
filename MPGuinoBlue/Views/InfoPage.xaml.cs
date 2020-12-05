using MPGuinoBlue.ViewModels;
using Shiny.BluetoothLE.Central;
using Xamarin.Forms;

namespace MPGuinoBlue.Views
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage(IPeripheral peripheral)
        {
            InitializeComponent();
            BindingContext = new InfoPageViewModel(peripheral);


        }
    }
}
