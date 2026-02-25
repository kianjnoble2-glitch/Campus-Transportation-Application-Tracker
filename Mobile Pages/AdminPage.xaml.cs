using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Firebase.Database;
using BCrypt.Net;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Text.RegularExpressions;

namespace Kats
{
    public partial class AdminPage : ContentPage
    {
        private Routes? _selectedRoute; // Nullable Routes
        private string? _selectedRouteDisplayText; // Nullable string

        private FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        private List<Routes>? Routes { get; set; } // List of routes
        private Stop? _selectedStop;
        public List<Users>? Users { get; set; }
        private const int MaxPhoneNumberLength = 10;

        public AdminPage()
        {
            InitializeComponent();
            LoadUsers();
            LoadRoutes();
            BindingContext = this;
            ChildrenCountPicker.SelectedIndexChanged += OnChildrenCountChanged;
           
            
        }
        private async void LoadRoutes()
        {
            // Asynchronous loading of routes
            var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var routes = await firebase
                .Child("Routes")
                .OnceAsync<Routes>(); // Fetching the routes from Firebase

            // Create a list to hold the route display items
            var routeDisplayItems = new List<RouteDisplayItem>();

            // Loop through the fetched routes and use the keys (e.g., Route1, Route2) as display names
            foreach (var route in routes)
            {
                routeDisplayItems.Add(new RouteDisplayItem
                {
                    Display = route.Key, // Use the key (e.g., Route1) as the display text
                    Route = route.Object // The actual route object
                });
            }

            // Assign the list to the picker’s ItemsSource
            RoutePicker.ItemsSource = routeDisplayItems;
        }

        private async void OnRouteChanged(object sender, EventArgs e)
        {
            // Get the selected route from the picker
            if (RoutePicker.SelectedItem is RouteDisplayItem selectedRouteItem)
            {
                // Store the selected route
                _selectedRoute = selectedRouteItem.Route;

                if (_selectedRoute != null) // Check if _selectedRoute is not null
                {
                    // Fetch the stops (areas) for the selected route
                    var selectedRouteKey = selectedRouteItem.Display; // The key like "Route1"
                    var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
                    var routeStops = await firebase
                        .Child($"Routes/{selectedRouteKey}/Stops")
                        .OnceAsync<Stop>();

                    // Display a popup with the list of areas (stops)
                    var areaNames = routeStops.Select(stop => stop.Object?.Area ?? "Unknown Area").ToArray();
                    var selectedArea = await DisplayActionSheet("Select your stop", "Cancel", null, areaNames);

                    // Check if the user selected an area
                    if (selectedArea != "Cancel" && !string.IsNullOrEmpty(selectedArea))
                    {
                        // Store the selected area for future use
                        _selectedStop = routeStops.FirstOrDefault(stop => stop.Object?.Area == selectedArea)?.Object;

                        // You can now use _selectedStop to know the user's stop on the route
                        Console.WriteLine($"Selected Stop: {_selectedStop?.Area} at Latitude: {_selectedStop?.Coordinates?.Latitude}, Longitude: {_selectedStop?.Coordinates?.Longitude}");
                    }
                }
            }
        }
        // Helper class to display route name and the route object
        private class RouteDisplayItem
        {
            public string? Display { get; set; } // The key (e.g., Route1)
            public Routes? Route { get; set; } // The actual route object

            public override string ToString()
            {
                return Display ?? string.Empty; // Display the key in the picker
            }
        }
        public void LoadUsers()
        {
            // Asynchronous loading of users
            var users = client.Child("Users").OnceAsync<Users>();
            Users = users.Result.Select(x => x.Object).ToList();
            UserRolesPicker.ItemsSource = Users;

            // Set default selected item (e.g., first item in the list)
            if (Users.Any())
            {
                UserRolesPicker.SelectedIndex = 0;
            }
        }

        private void OnUserRoleChanged(object sender, EventArgs e)
        {
            // Show the appropriate sections based on the selected role
            if (UserRolesPicker.SelectedItem is Users selectedUser)
            {
                if (selectedUser.UserRole == "Parent")
                {
                    ParentStackLayout.IsVisible = true;
                    AddParent.IsVisible = true;
                }
                else
                {
                    ParentStackLayout.IsVisible = false;
                    AddParent.IsVisible = false;
                }
            }
            else
            {
                // If no role is selected, hide the other fields
                ParentStackLayout.IsVisible = false;
                AddParent.IsVisible = false;
            }
        }

        // Example for replacing the content in case of readonly collection
        private void OnChildrenCountChanged(object? sender, EventArgs e)
        {
            var newChildrenLayout = new StackLayout();

            if (ChildrenCountPicker.SelectedItem is int count)
            {
                for (int i = 0; i < count; i++)
                {
                    newChildrenLayout.Children.Add(new Entry { Placeholder = $"Child {i + 1} Name", AutomationId = $"ChildName{i}" });
                    newChildrenLayout.Children.Add(new Entry { Placeholder = $"Child {i + 1} School Name", AutomationId = $"ChildSchoolName{i}" });
                    newChildrenLayout.Children.Add(new Entry { Placeholder = $"Child {i + 1} Grade", AutomationId = $"ChildGrade{i}" });
                    newChildrenLayout.Children.Add(new Entry { Placeholder = $"Child {i + 1} Bus Number", AutomationId = $"ChildBusNumber{i}" });
                }
            }

       
           
        }

        private void OnPhoneNumberTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null)
                return;

            // Use regex to allow only numbers
            var newText = string.Concat(e.NewTextValue.Where(char.IsDigit));

            // Limit the text to the maximum length allowed
            if (newText.Length > MaxPhoneNumberLength)
            {
                newText = newText.Substring(0, MaxPhoneNumberLength);
            }

            // Set the text back to the entry if it was changed
            if (entry.Text != newText)
            {
                entry.Text = newText;
            }
        }
        private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry == null)
                return;

            // Simple email validation pattern
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            var isValid = Regex.IsMatch(e.NewTextValue, emailPattern);

            // Optionally, you can display a warning if the email is invalid
            if (!isValid && !string.IsNullOrEmpty(e.NewTextValue))
            {
                entry.TextColor = Colors.Red; // Indicate invalid email
            }
            else
            {
                entry.TextColor = Colors.Black; // Reset color for valid email
            }
        }



        private async void OnAddParent(object sender, EventArgs e)
        {
            if (UserRolesPicker.SelectedItem is Users user && RoutePicker.SelectedItem is RouteDisplayItem selectedRouteItem)
            {
                // Hash the password before saving it
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(ParentPassword.Text);

                // Create a Parent object with all required fields set
                var parent = new Parent
                {
                    Name = ParentName.Text,
                    IsMother = Mother.IsChecked,
                    IsFather = Father.IsChecked,
                    IsGurdian = Gurdian.IsChecked,
                    PhoneNumber = ParentPhone.Text,
                    Email = ParentEmail.Text,
                    Password = hashedPassword, // Store the hashed password
                    user = user,
                    Route = selectedRouteItem.Route, // Set the route here
                    Children = new List<Children>()
                };

                // Add children entries
                for (int i = 0; i < ChildrenStackLayout.Children.Count; i += 4)
                {
                    var childNameEntry = ChildrenStackLayout.Children[i] as Entry;
                    var childSchoolNameEntry = ChildrenStackLayout.Children[i + 1] as Entry;
                    var childGradeEntry = ChildrenStackLayout.Children[i + 2] as Entry;
                    var childBusNumberEntry = ChildrenStackLayout.Children[i + 3] as Entry;

                    parent.Children.Add(new Children
                    {
                        Name = childNameEntry?.Text,
                        SchoolName = "Safe S Secondary",
                        Grade = childGradeEntry?.Text,
                        BusNumber = childBusNumberEntry?.Text
                    });
                }

                // Save parent and children to Firebase
                await client.Child("Parent").PostAsync(parent);

                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // Handle the case where no user or route is selected
                await DisplayAlert("Error", "Please select a user role and a route.", "OK");
            }
        }
    }
}
