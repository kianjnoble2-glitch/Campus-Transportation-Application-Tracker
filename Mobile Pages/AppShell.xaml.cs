namespace Kats
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(SplashPage), typeof(SplashPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(ParentPage), typeof(ParentPage));
            Routing.RegisterRoute(nameof(ParentMainPage), typeof(ParentMainPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(DriverMainPage), typeof(DriverMainPage));
            Routing.RegisterRoute(nameof(DriverPage), typeof(DriverPage));
            Routing.RegisterRoute(nameof(ChatPage), typeof(ChatPage));
        }
    }
}
