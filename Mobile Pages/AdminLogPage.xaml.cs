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
namespace Kats
{
    public partial class AdminLogPage : ContentPage
    {
        private AuthenticationService authService;

        public AdminLogPage()
        {
            InitializeComponent();
            authService = new AuthenticationService();
        }


        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            try
            {
                string userId = await authService.LoginAsync(EmailEntry.Text, PasswordEntry.Text);
                await DisplayAlert("Success", "Logged in successfully!", "OK");

                // Navigate to main page after successful login
                await Shell.Current.GoToAsync("//ParentMainPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
            }

        }
    }


}
