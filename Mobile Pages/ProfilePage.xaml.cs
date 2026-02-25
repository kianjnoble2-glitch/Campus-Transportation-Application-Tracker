
/* Unmerged change from project 'Kats (net8.0-maccatalyst)'
Before:
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Firebase.Database;
using Firebase.Database.Query;
After:
using System.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System.Maui.Controls;
using Firebase.Linq;
using System.Threading.Tasks;
*/

/* Unmerged change from project 'Kats (net8.0-android)'
Before:
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Firebase.Database;
using Firebase.Database.Query;
After:
using System.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System.Maui.Controls;
using Firebase.Linq;
using System.Threading.Tasks;
*/

/* Unmerged change from project 'Kats (net8.0-ios)'
Before:
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Firebase.Database;
using Firebase.Database.Query;
After:
using System.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System.Maui.Controls;
using Firebase.Linq;
using System.Threading.Tasks;
*/
using Firebase.Database;
using System.Collections.ObjectModel;

namespace Kats
{
    public partial class ProfilePage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        public ObservableCollection<Parent> ParentList { get; set; } = new ObservableCollection<Parent>();
        public ObservableCollection<Children> ChildrenList { get; set; } = new ObservableCollection<Children>();

        public ProfilePage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserDetails();
        }

        private async void LoadUserDetails()
        {
            var loggedInUserEmail = UserSession.LoggedInUserEmail;

            if (loggedInUserEmail != null)
            {
                var parents = await client.Child("Parent").OnceAsync<Parent>();
                var parent = parents.FirstOrDefault(p => p.Object.Email == loggedInUserEmail)?.Object;

                if (parent != null)
                {
                    ParentList.Clear();
                    ChildrenList.Clear();

                    ParentList.Add(parent);
                    foreach (var child in parent.Children)
                    {
                        ChildrenList.Add(child);
                    }
                }
            }
        }

        private async void OnHomeTapped(object sender, EventArgs e)
        {
            
            if (sender is Label label)
            {
                // Scale animation
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                // Navigate to Home page
                await Navigation.PushAsync(new ParentMainPage());
            }
        }
        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                // Scale animation Label.GestureRecognizers>                           
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                // Navigate to Home page
                await Navigation.PushAsync(new ProfilePage());
            }

        }

        private async void OnSettingsTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                // Scale animation Label.GestureRecognizers> "OnSettingsTapped"                                 
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                // Navigate to Home page
                await Navigation.PushAsync(new SettingsPage());
            }

        }
    }
}
