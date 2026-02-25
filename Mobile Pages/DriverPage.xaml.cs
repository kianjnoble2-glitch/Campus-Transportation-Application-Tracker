/* Unmerged change from project 'Kats (net8.0-maccatalyst)'
Before:
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
After:
using Microsoft.Maui.Controls;
using System.Maui.Controls;
*/

/* Unmerged change from project 'Kats (net8.0-android)'
Before:
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
After:
using Microsoft.Maui.Controls;
using System.Maui.Controls;
*/

/* Unmerged change from project 'Kats (net8.0-ios)'
Before:
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
After:
using Microsoft.Maui.Controls;
using System.Maui.Controls;
*/
using Firebase.Database;

namespace Kats
{
    public partial class DriverPage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");

        public DriverPage()
        {
            InitializeComponent();

        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(500);
            EmailEntry.Text = "kianjnoble1@gmail.com";
            PasswordEntry.Text = "markhamdamn";
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            bool isAuthenticated = await AuthenticateParent(email, password);

            if (isAuthenticated)
            {
                await DisplayAlert("Success", "Login successful!", "OK");
                // Navigate to ParentMainPage with email query parameter
                // After successful login
                UserSession.LoggedInUserEmail = email;
                await Shell.Current.GoToAsync($"{nameof(DriverMainPage)}?email={Uri.EscapeDataString(email)}");
            }
            else
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
            }
        }
        private async Task<bool> AuthenticateParent(string email, string password)
        {
            var drivers = await client.Child("Driver").OnceAsync<Driver>();
            var driver = drivers.FirstOrDefault(p => p.Object.Email == email);

            if (driver != null)
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, driver.Object.Password);
                }
                catch (BCrypt.Net.SaltParseException ex)
                {
                    Console.WriteLine("SaltParseException: " + ex.Message);
                    Console.WriteLine("Password from DB: " + driver.Object.Password);
                }

            }

            return false;
        }

    }


}
