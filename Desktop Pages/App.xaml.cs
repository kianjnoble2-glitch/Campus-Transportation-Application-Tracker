namespace AdminApp
{
    public partial class App : Application
    {
        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzQyNTU2OEAzMjM2MmUzMDJlMzBmT3VrUHg3S2wydmR6aDhOdlVQTnNtR1NVT3hzZlBuWjdhWk0wNHpab09JPQ==");
            InitializeComponent();

            MainPage = new AppShell();
            //MainPage = new DashboardPage();
        }
    }
}
