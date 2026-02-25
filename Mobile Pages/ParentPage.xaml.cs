using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;
using Firebase;
using FirebaseAdmin;
using Firebase.Database.Query;
using System.Threading.Tasks;
using Firebase.Database;



namespace Kats
{

    public partial class ParentPage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");

        public ParentPage()
        {
            InitializeComponent();
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
                await Shell.Current.GoToAsync($"{nameof(ParentMainPage)}?email={Uri.EscapeDataString(email)}");
            }
            else
            {
                await DisplayAlert("Error", "Invalid email or password.", "OK");
            }
        }

     

        private async Task<bool> AuthenticateParent(string email, string password)
        {
            var parents = await client.Child("Parent").OnceAsync<Parent>();
            var parent = parents.FirstOrDefault(p => p.Object.Email == email);

            if (parent != null)
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, parent.Object.Password);
                }
                catch (BCrypt.Net.SaltParseException ex)
                {
                    Console.WriteLine("SaltParseException: " + ex.Message);
                    Console.WriteLine("Password from DB: " + parent.Object.Password);
                }

            }

            return false;
        }
    }
}
