using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Maps;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Platform;

namespace Kats
{
    [QueryProperty(nameof(Email), "email")]
    public partial class DriverMainPage : ContentPage
    {
        private string? parentEmail;
        private string? parentName;
        private string? email;
        private const string GoogleMapsApiKey = "AIzaSyD4iT2YyOkzbobwrHaLeCNiWbG8TyLyzic";

        // ObservableCollection to store stops
        public ObservableCollection<Stop> Stops { get; set; } = new ObservableCollection<Stop>();

        // ETA properties
        public Dictionary<string, string> StopETAs { get; set; } = new Dictionary<string, string>();

        private string stop1ETA = string.Empty;
        public string Stop1ETA
        {
            get => stop1ETA;
            set
            {
                stop1ETA = value;
                OnPropertyChanged(nameof(Stop1ETA));
            }
        }

        private string stop2ETA = string.Empty;
        public string Stop2ETA
        {
            get => stop2ETA;
            set
            {
                stop2ETA = value;
                OnPropertyChanged(nameof(Stop2ETA));
            }
        }

        private string stop3ETA = string.Empty;
        public string Stop3ETA
        {
            get => stop3ETA;
            set
            {
                stop3ETA = value;
                OnPropertyChanged(nameof(Stop3ETA));
            }
        }

        // Distance properties
        private string stop1Distance = string.Empty;
        public string Stop1Distance
        {
            get => stop1Distance;
            set
            {
                stop1Distance = value;
                OnPropertyChanged(nameof(Stop1Distance));
            }
        }

        private string stop2Distance = string.Empty;
        public string Stop2Distance
        {
            get => stop2Distance;
            set
            {
                stop2Distance = value;
                OnPropertyChanged(nameof(Stop2Distance));
            }
        }

        private string stop3Distance = string.Empty;
        public string Stop3Distance
        {
            get => stop3Distance;
            set
            {
                stop3Distance = value;
                OnPropertyChanged(nameof(Stop3Distance));
            }
        }

        // Time in normal calculation
        private string stop1ETA_ = string.Empty;
        public string Stop1ETA_
        {
            get => stop1ETA_;
            set
            {
                stop1ETA_ = value;
                OnPropertyChanged(nameof(Stop1ETA_));
            }
        }

        private string stop2ETA_ = string.Empty;
        public string Stop2ETA_
        {
            get => stop2ETA_;
            set
            {
                stop2ETA_ = value;
                OnPropertyChanged(nameof(Stop2ETA_));
            }
        }

        private string stop3ETA_ = string.Empty;
        public string Stop3ETA_
        {
            get => stop3ETA_;
            set
            {
                stop3ETA_ = value;
                OnPropertyChanged(nameof(Stop3ETA_));
            }
        }

        public DriverMainPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadGoogleMapsRoute();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            email = UserSession.LoggedInUserEmail;
            LoadCheckIns();
            LoadStops(email);
        }

        private void LoadGoogleMapsRoute()
        {
            // HTML content that will be loaded into the WebView
            var htmlSource = new HtmlWebViewSource
            {
                Html = @"
        <!DOCTYPE html>
        <html>
          <head>
            <style>
              #map {
                height: 100%;
                width: 100%;
              }
              html, body {
                height: 100%;
                margin: 0;
                padding: 0;
              }
            </style>
            <script src='https://maps.googleapis.com/maps/api/js?key=AIzaSyD4iT2YyOkzbobwrHaLeCNiWbG8TyLyzic'></script>
            <script>
              function initMap() {
                const startLocation = { lat: -25.997747931190926, lng: 28.12960081491529 };  // Start location coordinates
                const stop1 = { lat: -25.997141428448874, lng: 28.130661185779953 };
                const stop2 = { lat: -25.996009810514305, lng: 28.133340257705186 };
                const destination = { lat: -25.99548094900603, lng: 28.135657797883496 };

                // Initialize the map centered on the start location
                const map = new google.maps.Map(document.getElementById('map'), {
                  center: startLocation,
                  zoom: 14
                });

                // Add route directions
                const directionsService = new google.maps.DirectionsService();
                const directionsRenderer = new google.maps.DirectionsRenderer({
                  map: map
                });

                const request = {
                  origin: startLocation,
                  destination: destination,
                  waypoints: [
                    { location: stop1 },
                    { location: stop2 }
                  ],
                  travelMode: google.maps.TravelMode.DRIVING
                };

                directionsService.route(request, function(result, status) {
                  if (status == 'OK') {
                    directionsRenderer.setDirections(result);
                    const routePath = result.routes[0].overview_path;  // Extract the route path
                    simulateSmoothMovement(routePath, map);
                  }
                });

                // Car icon marker configuration
                const carIcon = {
                  url: 'https://cdn-icons-png.flaticon.com/512/744/744465.png', // Example car icon
                  scaledSize: new google.maps.Size(30, 30)  // Scale the car icon
                };

                // Add car icon marker at the start location
                const marker = new google.maps.Marker({
                  position: startLocation,
                  map: map,
                  icon: carIcon
                });

               // Function to simulate smooth movement along the route path
function simulateSmoothMovement(routePath, map) {
  let step = 0;
  const numSteps = 300;  // Increase the number of steps for smoother movement
  const timePerStep = 10;  // Milliseconds per step (smaller value for smoother motion)

  function moveCar() {
    if (step + 1 < routePath.length) {
      const startLatLng = routePath[step];
      const endLatLng = routePath[step + 1];
      const deltaLat = (endLatLng.lat() - startLatLng.lat()) / numSteps;
      const deltaLng = (endLatLng.lng() - startLatLng.lng()) / numSteps;

      let stepIndex = 0;

      function animate() {
        if (stepIndex < numSteps) {
          const newLat = startLatLng.lat() + deltaLat * stepIndex;
          const newLng = startLatLng.lng() + deltaLng * stepIndex;
          const newPosition = new google.maps.LatLng(newLat, newLng);
          marker.setPosition(newPosition);
          stepIndex++;
          requestAnimationFrame(animate);  // Use requestAnimationFrame for smooth animation
        } else {
          step++;  // Move to the next route point

          // Check if the car has arrived at Stop 2 (index 3)
          if (step === 3) {  // Stop 2 is the 4th point (index 3)
            alert('Arrived at Stop 2');
            // Delay for 10 seconds before moving to the next destination
            setTimeout(() => {
              moveCar();  // Move to the next point after the delay
            }, 10000);  // 10-second delay
          } else {
            moveCar();  // Recursive call to move to the next point
          }
        }
      }

      animate();  // Start the animation
    }
  }

  moveCar();  // Start the car movement
}

                // Start the movement simulation 5 seconds after the map is loaded
                setTimeout(function() {
                  directionsService.route(request, function(result, status) {
                    if (status === 'OK') {
                      const routePath = result.routes[0].overview_path;  // Extract the route path again
                      simulateSmoothMovement(routePath, map);  // Start the movement
                    }
                  });
                }, 5000);  // 5-second delay
              }
            </script>
          </head>
          <body onload='initMap()'>
            <div id='map'></div>
          </body>
        </html>"
            };

            // Set the WebView source to load the custom HTML
            MapWebView.Source = htmlSource;
        }


        public string? Email
        {
            get => email;
            set
            {
                email = value;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    LoadGoogleMapsRoute();
                    LoadStops(email);
                    LoadCheckIns();
                }
                else
                {
                    email = UserSession.LoggedInUserEmail;
                    LoadGoogleMapsRoute();
                    LoadStops(email);
                    LoadCheckIns();
                }
            }
        }

        private async void LoadCheckIns()
        {
            var firebaseClient = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");

            // Fetch the parent data based on the logged-in email
            var parentsData = (await firebaseClient
                .Child("Parent")
                .OnceAsync<Parent>());

            if (true)
            {
                // var parentChildren = parentData.Object.Children.Select(c => c.Name).ToList();

                // Fetch CheckIns from Firebase
                var checkIns = (await firebaseClient
                    .Child("CheckIns")
                    .OnceAsync<CheckIn>())
                    .OrderByDescending(c => DateTime.Parse(c.Object.TimeStamp)) // Sort by timestamp
                    .ToList();

                // Clear existing check-ins from the layout
                CheckInStackLayout.Children.Clear();

                // Dynamically create UI elements for each check-in
                foreach (var checkIn in checkIns)
                {
                    // Create a card-like layout for each check-in
                    var card = new Frame
                    {
                        CornerRadius = 10,
                        HasShadow = true,
                        BackgroundColor = Color.FromArgb("#C68884"),
                        Padding = new Thickness(10),
                        Margin = new Thickness(5, 10)
                    };

                    var childNameLabel = new Label
                    {
                        Text = checkIn.Object.Child.Name,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("White")
                    };

                    var checkedInLabel = new Label
                    {
                        Text = $"Checked In: {checkIn.Object.CheckedIn}",
                        FontSize = 16,
                        TextColor = Color.FromArgb("#666666")
                    };

                    var timestampLabel = new Label
                    {
                        Text = $"Date: {checkIn.Object.TimeStamp}",
                        FontSize = 14,
                        TextColor = Color.FromArgb("#999999")
                    };

                    // Stack the labels vertically
                    var checkInLayout = new StackLayout
                    {
                        Padding = new Thickness(0, 5),
                        Children = { childNameLabel, checkedInLabel, timestampLabel }
                    };

                    // Add the check-in layout to the card
                    card.Content = checkInLayout;

                    // Add the card to the main StackLayout
                    CheckInStackLayout.Children.Add(card);
                }
            }
        }

        private async void LoadStops(string email)
        {
            var firebaseClient = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var driverdata = (await firebaseClient
                .Child("Driver")
                .OnceAsync<Driver>())
                .Where(p => p.Object.Email == email)
                .FirstOrDefault();

            if (driverdata != null && driverdata.Object?.bus.Route?.Stops != null)
            {
                Stops.Clear();
                foreach (var stop in driverdata.Object.bus.Route.Stops)
                {
                    if (stop.Value != null)
                    {
                        //await DisplayAlert("Test", stop.Value.Area, "OK");
                        Stops.Add(stop.Value);
                    }                }

            }
            else
            {
                var route = (await firebaseClient.Child("Routes").OnceAsync<Routes>()).FirstOrDefault(p => p.Key == "Route1")?.Object;
                var stops = route.Stops;
                foreach(var stop in stops) 
                {
                    
                    Stops.Add(stop.Value);
                }
            }
        }



        private void OnExitTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                var activity = Platform.CurrentActivity;
                activity?.MoveTaskToBack(true);
            }
        }


        private async void OnChatTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                await Navigation.PushAsync(new ChatPage(parentName, parentEmail));
            }
        }

        private async void OnSettingsTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                await Navigation.PushAsync(new SettingsPage());
            }
        }

        private async void OnNotificationsTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                await Navigation.PushAsync(new NotificationsPage());
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                await label.ScaleTo(1.2, 100);  // Scale up
                await label.ScaleTo(1.0, 100);  // Scale back to original size

                await Navigation.PushAsync(new DriverProfilePage());
            }
        }
    }
}

