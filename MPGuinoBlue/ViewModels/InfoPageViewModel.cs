using MPGuinoBlue.Views;
using Shiny.BluetoothLE.Central;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using IGattCharacteristic = Shiny.BluetoothLE.Central.IGattCharacteristic;
using IPeripheral = Shiny.BluetoothLE.Central.IPeripheral;

[assembly: Xamarin.Forms.Dependency(typeof(MPGuinoBlue.ViewModels.InfoPageViewModel))]


namespace MPGuinoBlue.ViewModels
{

    public class InfoPageViewModel : INotifyPropertyChanged
    {
        private const string Message = "poop";
        IDisposable _perifDisposable;
        public IGattCharacteristic _savedCharacteristic;


        public bool IsReadyToPrint { get; set; }
        public bool pies { get; set; }
        public string TextToSend { get; set; }
        public decimal Tspeed { get; set; }
        public decimal Tvolts { get; set; }
        public decimal Tremfuel { get; set; }
        public decimal Ttripodo { get; set; }
        public decimal Ttankodo { get; set; }
        public decimal Tdte { get; set; }
        public decimal Trpm { get; set; }
        public decimal Tinstant { get; set; }
        public static decimal Tinstant1 { get; set; } = 1;
        public decimal Ttrip100 { get; set; }
        public static decimal Ttrip1001 { get; set; } = 1;
        public decimal Ttank100 { get; set; }
        public decimal Ttripfuel { get; set; }



        public long Scontrast { get; set; } = 0;
        public long SvssPulses { get; set; } = 0;
        public long SmicroSec { get; set; } = 0;
        public long SperRev { get; set; } = 0;
        public long STripReset { get; set; } = 0;
        public long Stanksize { get; set; } = 0;
        public long Sinjsettle { get; set; } = 0;
        public long Svsspause { get; set; } = 0;
        public long SinjEdge { get; set; } = 0;
        public long Svolt { get; set; } = 0;

        public string flagg { get; set; }

        public string TextToPrint { get; set; }

        public ICommand SettingCommand1 { get; set; }
        public ICommand SettingCommand2 { get; set; }
        public ICommand SettingCommand3 { get; set; }
        public ICommand SettingCommand4 { get; set; }
        public ICommand SettingCommand5 { get; set; }
        public ICommand SettingCommand6 { get; set; }
        public ICommand SettingCommand7 { get; set; }
        public ICommand SettingCommand8 { get; set; }
        public ICommand SettingCommand9 { get; set; }
        public ICommand SettingCommand10 { get; set; }


        public int DelayFig { get; set; } = 500;

        public string Textback { get; set; }
        public string speed { get; set; }

        public string Textin { get; set; }
        public string speedunit { get; set; } = "km/h";
        public string distunit { get; set; } = "kms";
        public string fuelunit { get; set; } = "litres";
        public string econunit { get; set; } = "L/100kms";


        public bool Endd = false;

        public ICommand PrintCommand { get; set; }
        public ICommand SettingCommand { get; set; }
        public ICommand TankCommand { get; set; }
        public ICommand TripCommand { get; set; }
        public ICommand Disconnect { get; set; }
        public ICommand ConnectDeviceCommand { get; set; }


        public InfoPageViewModel(IPeripheral peripheral)
        {

            SettingCommand1 = new Command(savesettingsQ);
            SettingCommand2 = new Command(savesettingsR);
            SettingCommand3 = new Command(savesettingsS);
            SettingCommand4 = new Command(savesettingsT);
            SettingCommand5 = new Command(savesettingsU);
            SettingCommand6 = new Command(savesettingsV);
            SettingCommand7 = new Command(savesettingsW);
            SettingCommand8 = new Command(savesettingsX);
            SettingCommand9 = new Command(savesettingsY);
            SettingCommand10 = new Command(savesettingsZ);

            SettingCommand = new Command<IPeripheral>(SettingsView);
            TripCommand = new Command<IPeripheral>(ResetTrip);
            TankCommand = new Command<IPeripheral>(ResetTank);
            ConnectDeviceCommand = new Command<IPeripheral>(ConnectPrinter);
            Disconnect = new Command<IPeripheral>(DiscPrinter);
            ConnectDeviceCommand.Execute(peripheral);
        }





        void DiscPrinter(IPeripheral selectedPeripheral)
        {
            Endd = true;
            Thread.SpinWait(200);
            //if (selectedPeripheral.IsConnected())
            //  selectedPeripheral.CancelConnection();
            //flagD = true;
            Application.Current.MainPage.Navigation.PopAsync();
        }

        void ConnectPrinter(IPeripheral selectedPeripheral)
        {
            if (!selectedPeripheral.IsConnected())
                selectedPeripheral.Connect();

            _perifDisposable = selectedPeripheral.WhenAnyCharacteristicDiscovered().Subscribe((characteristic) =>
            {
                System.Diagnostics.Debug.WriteLine(characteristic.Description);
                if (characteristic.CanWrite() && characteristic.CanRead() && characteristic.CanNotify())
                {
                    IsReadyToPrint = true;
                    _savedCharacteristic = characteristic;


                    conn();
                    TriggerSettings();
                    Thread.SpinWait(800);

                    flagg = "!!";
                    TextToPrint = "0";
                    PrintText();
                    Thread.SpinWait(300);
                    System.Diagnostics.Debug.WriteLine($"Writing {characteristic.Uuid} - {characteristic.CanRead()} - {characteristic.CanIndicate()} - {characteristic.CanNotify()}");
                    _perifDisposable.Dispose();
                }
            });
        }



        void conn()
        {


            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken Ender = source.Token;
            //ShowMessage("Hey", "We re-connected");
            if (_savedCharacteristic != null)
            {
                _savedCharacteristic.Notify(false, true).Subscribe(a =>
                 { SetReadValue(a); if (Endd == true) source.Cancel(); Endd = false; },
                                                                                         ex => Application.Current.MainPage.Navigation.PopAsync(), Ender);
            }
            if (_savedCharacteristic == null)
            {
                ShowMessage("Oops", "Please use with an MPGuino device");
            }
        }


        void SetReadValue(CharacteristicGattResult result) => Device.BeginInvokeOnMainThread(() =>
        {
            if (result.Data == null)
                Textin = "!EMPTY";

            else
                Textin = Encoding.UTF8.GetString(result.Data, 0, result.Data.Length);
            if (Textin == "ER") { ShowMessage("Oops", "Data didn't send, Try again..."); }
            Decode(Textin);
        });


        void Decode(string words)
        {
            string check;
            int number = 0;
            if (words.Length >= 2)
            {
                check = words.Remove(1);
                //var charsToRemove = new string[] { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "=", "-", "{", "}", "[", "]", ":", "; ", " ", "'", "<", ">", ",", ".", "?", "/", "`", "~" };
                // foreach (var c in charsToRemove)
                // {
                //    words = words.Replace(c, string.Empty);
                // }
                Int32.TryParse(words.Remove(0, 1), out number);
            }

            else check = "";

            //words.Remove (0, 1)

            if (check == "A") Tspeed = (Convert.ToDecimal(number)) / (decimal)100;
            if (check == "B") Tvolts = (Convert.ToDecimal(number)) / (decimal)1000;
            if (check == "C") Tremfuel = (Convert.ToDecimal(number)) / (decimal)100;
            if (check == "D") Ttripodo = (Convert.ToDecimal(number)) / (decimal)100;
            // if (Ttripodo >= 10000) Ttripodo = (
            if (check == "E") Ttankodo = (Convert.ToDecimal(number)) / (decimal)100;
            if (check == "F") Tdte = (Convert.ToDecimal(number)) / (decimal)100;
            if (check == "G") Trpm = (Convert.ToDecimal(number)); // (decimal)100;
            if (check == "H")
            {
                Tinstant = (Convert.ToDecimal(number)) / (decimal)100;
                if (Tspeed >= 1)
                {
                    Tinstant1 = (Convert.ToDecimal(number)) / (decimal)100;
                    TripPage.tick = 1;
                    TankPage.tick = 1;
                }
                else Tinstant1 = 0;
            }

            if (check == "I")
            {
                Ttrip100 = (Convert.ToDecimal(number)) / (decimal)100;
                Ttrip1001 = (Convert.ToDecimal(number)) / (decimal)100;
            }
            if (check == "J") Ttank100 = (Convert.ToDecimal(number)) / (decimal)100;
            if (check == "L") Ttripfuel = (Convert.ToDecimal(number)) / (decimal)100;

            if (check == "Q") Scontrast = (Convert.ToInt32(number));
            if (check == "R") SvssPulses = (Convert.ToInt32(number));
            if (check == "S") SmicroSec = (Convert.ToInt32(number));
            if (check == "T") SperRev = (Convert.ToInt32(number));
            if (check == "U") STripReset = (Convert.ToInt32(number));
            if (check == "V") Stanksize = (Convert.ToInt32(number));
            if (check == "W") Sinjsettle = (Convert.ToInt32(number));
            if (check == "X") Svsspause = (Convert.ToInt32(number));
            if (check == "Y") SinjEdge = (Convert.ToInt32(number));
            if (check == "Z") Svolt = (Convert.ToInt32(number));

            if (STripReset == 1)
            {
                if (Tspeed == 0) speed = "G/Hour"; else speed = "MPG";

                speedunit = "MPH";
                distunit = "miles";
                fuelunit = "gallons";
                econunit = "MPG";
            }
            if (STripReset == 2)
            {
                if (Tspeed == 0) speed = "L/Hour"; else speed = "L/100kms";

                speedunit = "km/h";
                distunit = "kms";
                fuelunit = "litres";
                econunit = "L/100kms";
            }
        }

        void TriggerSettings()
        {

            Endd = true;
            Thread.SpinWait(200);
            TextToSend = "MM";
            Thread.SpinWait(200);

            _savedCharacteristic?.Write(Encoding.UTF8.GetBytes($"{TextToSend}")).Subscribe(
                result =>
                {
                    Thread.SpinWait(200);

                    //ShowMessage("Yay", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToSend}"), 0, Encoding.UTF8.GetBytes($"{TextToSend}").Length));
                },
                exception =>
                {
                    //ShowMessage("Error", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToPrint}"), 0, Encoding.UTF8.GetBytes($"{TextToPrint}").Length));
                });
            conn();
            Thread.SpinWait(600);
            Endd = true;
            Thread.SpinWait(20);
        }
        void ResetTank(IPeripheral selectedPeripheral)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {

                string action = await App.Current.MainPage.DisplayActionSheet("Full or Partial refuel?", "Cancel", null, "Full Tank", "Partial Refuel");


                if (action == "Full Tank")
                {
                    Endd = true;
                    Thread.SpinWait(200);
                    TextToSend = "MT";
                    Thread.SpinWait(200);

                    _savedCharacteristic?.Write(Encoding.UTF8.GetBytes($"{TextToSend}")).Subscribe(
                        result =>
                        {
                            Thread.SpinWait(200);

                        //ShowMessage("Yay", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToSend}"), 0, Encoding.UTF8.GetBytes($"{TextToSend}").Length));
                    },
                        exception =>
                        {
                        //ShowMessage("Error", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToPrint}"), 0, Encoding.UTF8.GetBytes($"{TextToPrint}").Length));
                    });
                    conn();

                    Thread.SpinWait(600);
                }
                if (action == "Partial Refuel")
                {
                    Endd = true;
                    Thread.SpinWait(200);

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Partial Refuel", "How much fuel have you added to the tank *1000? (20.69L = 20,690)", "Save", "Cancel", "10,000L", 8, keyboard: Keyboard.Numeric);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsPART();
                            Thread.SpinWait(650);
                        }
                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                    conn();
                    Thread.SpinWait(600);
                }
            });
        }

        void ResetTrip(IPeripheral selectedPeripheral)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {

                pies = await App.Current.MainPage.DisplayAlert("Reset Trip", "Are you sure you would like to reset your trip?", "Ok", "Cancel");


                if (pies == true)
                {
                    Endd = true;
                    Thread.SpinWait(200);
                    TextToSend = "MR";
                    Thread.SpinWait(200);

                    _savedCharacteristic?.Write(Encoding.UTF8.GetBytes($"{TextToSend}")).Subscribe(
                        result =>
                        {
                            Thread.SpinWait(200);

                        //ShowMessage("Yay", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToSend}"), 0, Encoding.UTF8.GetBytes($"{TextToSend}").Length));
                    },
                        exception =>
                        {
                        //ShowMessage("Error", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToPrint}"), 0, Encoding.UTF8.GetBytes($"{TextToPrint}").Length));
                    });
                    conn();
                    Thread.SpinWait(600);
                }
            });
        }

        void SettingsView(IPeripheral selectedPeripheral)
        {
            TriggerSettings();
            Device.BeginInvokeOnMainThread(async () =>
            {
                string action = await App.Current.MainPage.DisplayActionSheet("Select a Setting to change", "Cancel", null, "VSS Pulses/KM", "Microsec/Litre", "Pulses/2Rev", "Imperial/Metric", "Tank Size", "Inj Delay", "VSS Delay", "Inj Type", "Volt Offset");

                conn();
                flagg = "!!";
                TextToPrint = "0";
                PrintText();
                Thread.SpinWait(300);

                if (action == "VSS Pulses/KM")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(SvssPulses);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("VSS Pulses/KM", "The amount of pulses recieved per KM", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsR();
                            Thread.SpinWait(650);
                        }
                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Microsec/Litre")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(SmicroSec);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Microsec/Litre", "How many Microseconds of injector time for 1 Litre", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsS();
                            Thread.SpinWait(50);
                        }
                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Pulses/2Rev")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(SperRev);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Pulses/2Rev", "Inj pulses per 2 engine cycles, for RPM readout", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsT();
                            Thread.SpinWait(50);
                        }
                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Imperial/Metric")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(STripReset);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Imperial/Metric", "1 for Imperial, 2 for Metric", "Save", "Cancel", crap, 1, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint == "1")
                        {
                            savesettingsU();
                            Thread.SpinWait(50);
                        }
                        if (TextToPrint == "2")
                        {
                            savesettingsU();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Tank Size")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(Stanksize);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Tank Size", "Tank size *1000 (50L = 50,000)", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsV();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Inj Delay")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(Sinjsettle);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Inj delay", "Micro second delay of Inj pulse signal for smothing", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsW();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "VSS Delay")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(Svsspause);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("VSS Delay", "For smothing VSS signal", "Save", "Cancel", crap, 8, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsX();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Inj Type")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(SinjEdge);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Injector Type", "0 Normal, 1 Inverse, 2 Peak and Hold", "Save", "Cancel", crap, 1, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint == "0")
                        {
                            savesettingsY();
                            Thread.SpinWait(50);
                        }
                        if (TextToPrint == "1")
                        {
                            savesettingsY();
                            Thread.SpinWait(50);
                        }
                        if (TextToPrint == "2")
                        {
                            savesettingsY();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }
                if (action == "Volt Offset")
                {
                    TriggerSettings();
                    string crap = Convert.ToString(Svolt);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        TextToPrint = await App.Current.MainPage.DisplayPromptAsync("Volt Offset", "Adjust this figure for Voltage accuracy", "Save", "Cancel", crap, 5, keyboard: Keyboard.Numeric, crap);
                        //CheckIfOK(TextToPrint);
                        if (TextToPrint != null)
                        {
                            savesettingsZ();
                            Thread.SpinWait(50);
                        }

                        conn();
                        flagg = "!!";
                        TextToPrint = "0";
                        PrintText();
                    });

                }




            });
        }

        async void CheckIfOK(string numeric)
        {
            await CheckCheck("Save Setting?", numeric, "OK", async () =>
               {

               });
        }

        public async Task CheckCheck(string title, string message, string buttonText, Action aftercallback)
        {
            bool answer = await App.Current.MainPage.DisplayAlert(title, message, buttonText, "Cancel");
            aftercallback?.Invoke();
            if (answer == true)
            {
                savesettingsQ();
                Thread.SpinWait(50);
                conn();
                flagg = "!!";
                TextToPrint = "0";
                PrintText();
            };
        }


        void savesettingsQ()
        {
            flagg = "Q";
            PrintText();
        }
        void savesettingsPART()
        {
            flagg = "@";
            PrintText();
        }

        void savesettingsR()
        {
            flagg = "R";
            PrintText();
        }
        void savesettingsS()
        {
            flagg = "S";
            PrintText();
        }
        void savesettingsT()
        {
            flagg = "T";
            PrintText();
        }
        void savesettingsU()
        {
            flagg = "U";
            PrintText();
        }
        void savesettingsV()
        {
            flagg = "V";
            PrintText();
        }
        void savesettingsW()
        {
            flagg = "W";
            PrintText();
        }
        void savesettingsX()
        {
            flagg = "X";
            PrintText();
        }
        void savesettingsY()
        {
            flagg = "Y";
            PrintText();

        }
        void savesettingsZ()
        {
            flagg = "Z";
            PrintText();
        }




        public void PrintText()
        {

            Endd = true;
            Thread.SpinWait(200);
            TextToSend = flagg + TextToPrint + "K" + TextToPrint + "K";

            _savedCharacteristic?.Write(Encoding.UTF8.GetBytes($"{TextToSend}")).Subscribe(
           result =>
           {
               Thread.Sleep(50);
               //ShowMessage("Yay", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToSend}"), 0, Encoding.UTF8.GetBytes($"{TextToSend}").Length));
           },
           exception =>
           {
               ShowMessage("Error", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes($"{TextToPrint}"), 0, Encoding.UTF8.GetBytes($"{TextToPrint}").Length));
           });
            conn();
        }

        public void ShowMessage(string title, string message)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.DisplayAlert(title, message, "Ok");
            });
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
