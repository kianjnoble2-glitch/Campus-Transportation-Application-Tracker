using Firebase.Database;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Kats
{
    [QueryProperty(nameof(Email), "email")]
    public partial class ParentMainPage : ContentPage
    {
        private string? email;
        private Location? currentLocation;

        public string? Email
        {
            get => email;
            set
            {
                email = value;
                if (!string.IsNullOrEmpty(email))
                {
                    LoadStops(email);
                }
            }
        }

        public ObservableCollection<Stop> Stops { get; set; } = new ObservableCollection<Stop>();

        public ParentMainPage()
        {
            InitializeComponent();
            BindingContext = this;
            GetCurrentLocationAsync();
        }

        private async void GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location != null)
                {
                    currentLocation = new Location(location.Latitude, location.Longitude);
                }
                else
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.High
                    });

                    if (location != null)
                    {
                        currentLocation = new Location(location.Latitude, location.Longitude);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"Error getting location: {ex.Message}");
            }
        }

        private async void LoadStops(string email)
        {
            var firebaseClient = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var parentData = (await firebaseClient
                .Child("Parent")
                .OnceAsync<Parent>())
                .Where(p => p.Object.Email == email)
                .FirstOrDefault();

            if (parentData != null && parentData.Object?.Route?.Stops != null)
            {
                Stops.Clear();
                foreach (var stop in parentData.Object.Route.Stops)
                {
                    if (stop.Value != null)
                    {
                        Stops.Add(stop.Value);
                    }
                }
                AddPinsToMap();
                DrawRoute();
            }
        }

        private void AddPinsToMap()
        {
            MyMap.Pins.Clear();

            foreach (var stop in Stops)
            {
                if (stop.Area != null && stop.Coordinates != null)
                {
                    var pin = new Pin
                    {
                        Label = stop.Area,
                        Address = stop.Area,
                        Location = new Location(stop.Coordinates.Latitude, stop.Coordinates.Longitude),
                        Type = PinType.Place
                    };

                    MyMap.Pins.Add(pin);
                }
            }

            var firstCoordinates = Stops.FirstOrDefault()?.Coordinates;
            if (firstCoordinates != null)
            {
                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(firstCoordinates.Latitude, firstCoordinates.Longitude),
                    Distance.FromMiles(1))); // Zoom level
            }
        }


        private void DrawRoute()
        {
            var polyline = new Polyline
            {
                StrokeColor = Colors.Blue,
                StrokeWidth = 3
            };

            foreach (var stop in Stops)
            {
                if (stop.Coordinates != null)
                {
                    polyline.Geopath.Add(new Location(stop.Coordinates.Latitude, stop.Coordinates.Longitude));
                }
            }

            MyMap.MapElements.Add(polyline);
        }

        private async void OnProfileClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(email))
            {
                await Shell.Current.GoToAsync($"{nameof(ProfilePage)}?email={Uri.EscapeDataString(email)}");
            }
        }



        private async void OnHomeTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100); // Scale up
                await label.ScaleTo(1.0, 100); // Scale back to original size
                await Navigation.PushAsync(new ParentMainPage());
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100); // Scale up
                await label.ScaleTo(1.0, 100); // Scale back to original size
                await Navigation.PushAsync(new ProfilePage());
            }
        }
        private async void OnChatTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100); // Scale up
                await label.ScaleTo(1.0, 100); // Scale back to original size
                await Navigation.PushAsync(new ChatPage("jeremybutton@gmail.com", "Jeremy Button"));
            }
        }

    }
}

