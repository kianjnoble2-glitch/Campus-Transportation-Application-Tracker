using Microsoft.Maui.Controls;

namespace Kats
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()

        {
            global::Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof(SplashPage));
            InitializeComponent();
            NavigateToLoginPage();
        }

        private async void NavigateToLoginPage()
        {
            await Task.Delay(3000); // Delay for 4 seconds
            if (Application.Current != null)
            {
                await Navigation.PushAsync(new LoginPage());
            }
        }
    }
}
