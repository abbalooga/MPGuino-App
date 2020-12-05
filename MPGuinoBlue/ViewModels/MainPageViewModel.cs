using MPGuinoBlue.ViewModels;
using MPGuinoBlue.Views;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Shiny;
using Shiny.BluetoothLE.Central;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MPGuinoBlue.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        IDisposable _scanDisposable, _connectedDisposable;
        ICentralManager _centralManager = Shiny.ShinyHost.Resolve<ICentralManager>();

        public bool IsScanning { get; set; }
        public bool automatic { get; set; } = true;
        public ObservableCollection<IPeripheral> Peripherals { get; set; } = new ObservableCollection<IPeripheral>();

        public ICommand GetDeviceListCommand { get; set; }
        ICommand SetAdapterCommand { get; set; }
        ICommand CheckPermissionsCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public IPeripheral _selectedPeripheral;
        public IPeripheral SelectedPeripheral
        {
            get
            {
                return _selectedPeripheral;
            }
            set
            {
                _selectedPeripheral = value;
                if (_selectedPeripheral != null)
                    OnSelectedPeripheral(_selectedPeripheral);
            }
        }

        private static ISettings AppSettings
        {
            get
            {
                if (CrossSettings.IsSupported)
                    return CrossSettings.Current;

                return null; // or your custom implementation 
            }
        }




        public static bool scanflag
        {
            get => AppSettings.GetValueOrDefault(nameof(scanflag), false);
            set => AppSettings.AddOrUpdateValue(nameof(scanflag), value);
        }
        public bool scanflagshow { set; get; } = scanflag;

        public static bool saveflag
        {
            get => AppSettings.GetValueOrDefault(nameof(saveflag), false);
            set => AppSettings.AddOrUpdateValue(nameof(saveflag), value);
        }
        public bool saveflagshow { set; get; } = saveflag;
        public static string savecharac
        {
            get => AppSettings.GetValueOrDefault(nameof(savecharac), "");
            set => AppSettings.AddOrUpdateValue(nameof(savecharac), value);
        }
        public static string savepage
        {
            get => AppSettings.GetValueOrDefault(nameof(savepage), "All Parameters");
            set => AppSettings.AddOrUpdateValue(nameof(savepage), value);
        }

        public MainPageViewModel()
        {
            GetDeviceListCommand = new Command(GetDeviceList);
            SetAdapterCommand = new Command(async () => await SetAdapter());
            CheckPermissionsCommand = new Command(async () => await CheckPermissions());
            CheckPermissionsCommand.Execute(null);

        }

        async Task CheckPermissions()
        {


            var status = await _centralManager.RequestAccess();
            if (status == AccessState.Denied)
            {
                await App.Current.MainPage.DisplayAlert("Permissions", "You need to have your bluetooth ON to use this app", "Ok");
                Xamarin.Essentials.AppInfo.ShowSettingsUI();
            }
            else
            {
                SetAdapterCommand.Execute(null);
            }
        }

        async Task SetAdapter()
        {
            var poweredOn = _centralManager.Status == AccessState.Available;
            if (!poweredOn && !_centralManager.TrySetAdapterState(true))
                await App.Current.MainPage.DisplayAlert("Cannot change bluetooth adapter state", "", "Ok");

            if (poweredOn)
            {
                GetConnectedDevices();

            }
        }

        void GetConnectedDevices()
        {
            _connectedDisposable = _centralManager.GetConnectedPeripherals().Subscribe(scanResult =>
 {
     scanResult.ToList().ForEach(
      item =>
      {
          if (!string.IsNullOrEmpty(item.Name))
              Peripherals.Add(item);

          if (item.Name == savecharac && saveflag == true) //saveflagshow
          {
              automatic = true;
              OnSelectedPeripheral(item);
              Device.BeginInvokeOnMainThread(async () =>
                         {
                             DeviceDisplay.KeepScreenOn = true; //await App.Current.MainPage.DisplayAlert("List Auto", item.Name, "Ok");
                         });
          }

      });

     _connectedDisposable?.Dispose();
 });


            if (_centralManager.IsScanning)
                _centralManager.StopScan();
            if (saveflagshow == true)
            {
                GetDeviceList();
            }
        }

        void OnSelectedPeripheral(IPeripheral peripheral)
        {
            if (scanflag == false || automatic == false)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    string action = await App.Current.MainPage.DisplayActionSheet("Choose a view", "Cancel", null, "All Parameters", "Trip View", "Tank View");

                    if (action == "All Parameters")
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                    {
                        DeviceDisplay.KeepScreenOn = true;

                        await App.Current.MainPage.Navigation.PushAsync(new InfoPage(peripheral));
                        AppSettings.AddOrUpdateValue(nameof(savepage), action);
                        AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);

                        SelectedPeripheral = null;
                    });
                    }
                    if (action == "Trip View")
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            //pp DeviceDisplay.KeepScreenOn = !DeviceDisplay.KeepScreenOn;
                            DeviceDisplay.KeepScreenOn = true;
                            await App.Current.MainPage.Navigation.PushAsync(new TripPage(peripheral));
                            AppSettings.AddOrUpdateValue(nameof(savepage), action);
                            AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);

                            SelectedPeripheral = null;
                        });
                    }
                    if (action == "Tank View")
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            DeviceDisplay.KeepScreenOn = true;
                            await App.Current.MainPage.Navigation.PushAsync(new TankPage(peripheral));
                            AppSettings.AddOrUpdateValue(nameof(savepage), action);
                            AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);

                            SelectedPeripheral = null;
                        });
                    }

                    _scanDisposable?.Dispose();
                    IsScanning = _centralManager.IsScanning;
                    SelectedPeripheral = null;

                });
            }
            if (scanflag == true && automatic == true)
            {
                if (savepage == "All Parameters")
                {

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        DeviceDisplay.KeepScreenOn = true;

                        await App.Current.MainPage.Navigation.PushAsync(new InfoPage(peripheral));

                        AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);
                        automatic = false;

                        SelectedPeripheral = null;
                    });
                }

                if (savepage == "Trip View")
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        DeviceDisplay.KeepScreenOn = true;

                        await App.Current.MainPage.Navigation.PushAsync(new TripPage(peripheral));

                        AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);
                        automatic = false;

                        SelectedPeripheral = null;
                    });

                }
                if (savepage == "Tank View")
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        DeviceDisplay.KeepScreenOn = true;

                        await App.Current.MainPage.Navigation.PushAsync(new TankPage(peripheral));

                        AppSettings.AddOrUpdateValue(nameof(savecharac), peripheral.Name);
                        automatic = false;

                        SelectedPeripheral = null;
                    });

                }


            }
        }


        void GetDeviceList()
        {
            if (_centralManager.IsScanning)
            {
                _scanDisposable?.Dispose();
            }
            else
            {
                if (_centralManager.Status == Shiny.AccessState.Available && !_centralManager.IsScanning)
                {
                    _scanDisposable = _centralManager.ScanForUniquePeripherals().Subscribe(scanResult =>
                    {
                        if (!string.IsNullOrEmpty(scanResult.Name) && !Peripherals.Contains(scanResult))
                            Peripherals.Add(scanResult);

                        if (scanResult.Name == savecharac && saveflag == true) //saveflagshow
                        {
                            OnSelectedPeripheral(scanResult);
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                    //await App.Current.MainPage.DisplayAlert("Scan Auto", scanResult.Name, "Ok");
                                });
                        }



                    });
                }
            }
            IsScanning = _centralManager.IsScanning;
        }
    }
}

namespace mpg.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {

        public static void savesaveswitch(ToggledEventArgs e)
        {
            AppSettings.AddOrUpdateValue(nameof(MainPageViewModel.saveflag), e.Value);

        }

        public static void savescanswitch(ToggledEventArgs e)
        {
            AppSettings.AddOrUpdateValue(nameof(MainPageViewModel.scanflag), e.Value);

        }
        public static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        public const string SettingsKey = "settings_key";
        public static readonly string SettingsDefault = string.Empty;

        #endregion


        public static string GeneralSettings
        {
            get
            {
                return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(SettingsKey, value);
            }
        }

    }
}
