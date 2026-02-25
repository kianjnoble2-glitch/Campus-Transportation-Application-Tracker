using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Maui.Controls;

namespace Kats
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzQyNTU2OEAzMjM2MmUzMDJlMzBmT3VrUHg3S2wydmR6aDhOdlVQTnNtR1NVT3hzZlBuWjdhWk0wNHpab09JPQ==");

            InitializeComponent();
         
            MainPage = new AppShell();
            

        }
        protected override async void OnStart()
        {
            await Shell.Current.GoToAsync("SplashPage");
        }

    }

}
