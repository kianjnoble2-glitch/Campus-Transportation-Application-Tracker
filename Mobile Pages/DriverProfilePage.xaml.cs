



using Firebase.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Maui.Graphics;


namespace Kats
{
    public partial class DriverProfilePage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        public ObservableCollection<DriverDisplay> DriverList { get; set; } = new ObservableCollection<DriverDisplay>();
        public ObservableCollection<Children> ChildrenList { get; set; } = new ObservableCollection<Children>();
        public class DriverDisplay : Driver
        {
            public string? BusNumber { get; set; }
        }
        public DriverProfilePage()
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
                var drivers = await client.Child("Driver").OnceAsync<Driver>();
                var driver = drivers.FirstOrDefault(p => p.Object.Email == loggedInUserEmail)?.Object;

                if (parents != null && driver != null)
                {
                    DriverList.Clear();
                    ChildrenList.Clear();
                    DriverList.Add(new DriverDisplay
                    {
                        bus = driver.bus,
                        BusNumber = driver?.bus?.BusNumber,
                        Email = driver?.Email,
                        Image = driver?.Image,
                        IsFemale = driver.IsFemale,
                        IsMale = driver.IsMale,
                        Name = driver?.Name,
                        Password = driver?.Password,
                        PhoneNumber = driver?.PhoneNumber,
                        Route = driver?.Route,
                        user = driver?.user                    
                    });
                    foreach (var par in parents)
                    {
                        foreach (var child in par.Object.Children)
                        {
                            if (child != null)
                            {
                                if (child.BusNumber == driver?.bus?.BusNumber)
                                {
                                    ChildrenList.Add(child);
                                }
                            }
                        }
                    }
                    studentsLabel.Text = ChildrenList.Count.ToString() + " STUDENTS";
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
                await Navigation.PushAsync(new DriverMainPage());
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
                await Navigation.PushAsync(new DriverProfilePage());
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