using Microsoft.Maui.Controls;

namespace Kats
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            StartRotation();
            AnimateArrow();
        }


        private async void OnParentButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ParentPage());
        }
        private async void OnDriverButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DriverPage());
        }
        private void StartRotation()
        {

            RotatingIcon.RotateTo(360, 2000, Easing.Linear)
                        .ContinueWith(t => RotatingIcon.RotateTo(0, 0, Easing.Linear))
                        .ContinueWith(t => StartRotation());
        }
        private async void AnimateArrow()
        {
            while (true)
            {
                await ArrowImage.FadeTo(0, 1000); // Fade out to opacity 0 over 1 second
                await ArrowImage.FadeTo(1, 1000); // Fade in to opacity 1 over 1 second
            }
        }

    }
}
