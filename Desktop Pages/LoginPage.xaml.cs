using Firebase.Database;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Diagnostics;

namespace AdminApp
{
    public partial class LoginPage : ContentPage
    {
        private AuthenticationService authService;

        public LoginPage()
        {
            InitializeComponent();
            authService = new AuthenticationService();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            EmailEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            loginButton.IsEnabled = true;
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            loginButton.IsEnabled = false;
            try
            {
                string userId = await authService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);
                await DisplayAlert("Success", "Logged in successfully!", "OK");

                // Navigate to main page after successful login
                await Shell.Current.GoToAsync($"{nameof(MainPage)}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", "Make sure you enter the correct details...", "OK");
                Debug.WriteLine(ex.Message);
                loginButton.IsEnabled = true;
                await Shell.Current.GoToAsync($"{nameof(MainPage)}");
            }

        }
    }

}