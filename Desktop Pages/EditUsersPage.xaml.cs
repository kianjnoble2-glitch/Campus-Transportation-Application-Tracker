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
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Popup;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Diagnostics;
using Grpc.Core;
using System.Reactive.Subjects;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;
using static GoogleApi.GoogleMaps;
using System.Collections;
using System.ComponentModel;
using Microsoft.Maui.Storage;

namespace AdminApp
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [QueryProperty(nameof(Email), "email")]

    public partial class EditUsersPage : ContentPage
    {
        public string? email;

        public string? Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
                // You might want to use this value in some logic if needed
            }
        }
        public ObservableCollection<Parent> ParentList { get; set; } = new ObservableCollection<Parent>();
        public ObservableCollection<Children> ChildrenList { get; set; } = new ObservableCollection<Children>();
        public ObservableCollection<Driver> DriverList { get; set; } = new ObservableCollection<Driver>();
        public ObservableCollection<Bus> BusList { get; set; } = new ObservableCollection<Bus>();
        public ObservableCollection<Routes> RouteList { get; set; } = new ObservableCollection<Routes>();

        private Routes? _selectedRoute; // Nullable Routes
        //private string? _selectedRouteDisplayText; // Nullable string
        private string _image = String.Empty;

        private FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        private List<Routes>? Routes { get; set; } // List of routes
        private Stop? _selectedStop;
        public List<Users>? Users { get; set; }
        private const int MaxPhoneNumberLength = 10;
        public int childrenCount;
        //SfPopup popupUserRoles = new SfPopup();
        public EditUsersPage()
        {
            InitializeComponent();
            //LoadUsers();
            //LoadRoutes();
            LoadSelectedUser();
            BindingContext = this;
            //ChildrenCountPicker.SelectedIndexChanged += OnChildrenCountChanged;
        }

        private async void LoadSelectedUser()
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

            await Task.Delay(200);
            if (email != string.Empty)
            {
                var _checkParents = await client.Child("Parent").OnceAsync<Parent>();
                var parent = _checkParents.FirstOrDefault(p => p.Object.Email == email)?.Object;
                var _checkDrivers = await client.Child("Driver").OnceAsync<Driver>();
                var driver = _checkDrivers.FirstOrDefault(p => p.Object.Email == email)?.Object;

                if (parent != null && driver == null)
                {
                    ParentList.Clear();
                    ChildrenList.Clear();
                    RouteList.Clear();
                    ParentList.Add(parent);

                    foreach (var child in parent.Children)
                    {
                        ChildrenList.Add(child);
                    }

                    // Create RouteDisplayItem of matching Route object
                    var userRoute = routeDisplayItems.Where(p => JsonConvert.SerializeObject(p.Route) == JsonConvert.SerializeObject(parent.Route)).FirstOrDefault();
                    if (userRoute != null)
                    {
                        // Display data of the user
                        ParentName.Text = parent.Name;
                        ParentEmail.Text = parent.Email;
                        ParentPhone.Text = parent.PhoneNumber;
                        childrenCountSlider.Value = parent.Children.Count;
                        UserRolesPicker.SelectedItem = parent.user;
                        Mother.IsChecked = parent.IsMother;
                        Father.IsChecked = parent.IsFather;
                        Gurdian.IsChecked = parent.IsGurdian;
                        LabelChosenRoute.Text = "Chosen Route: " + userRoute.Display;
                        _selectedRoute = userRoute.Route;
                        //RoutePicker.SelectedItem = userRoute;     Could require userRoute.Route instead, not sure, both are RouteDisplayItems
                        GenerateChildrenGrid(ChildrenList.Count);

                        if (String.IsNullOrWhiteSpace(parent.Image) == false)
                        {
                            _image = parent.Image;
                            avatarProfile.ImageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(parent.Image)));
                        }

                    }
                    else
                    {
                        await DisplayAlert("Error", "The route does not exist, contact administrators...", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                }
                else if (driver != null && parent == null)
                {
                    RoutePicker.ItemsSource = await LoadBuses();

                    // DRIVER STUFF HERE
                    DriverList.Clear();
                    DriverList.Add(driver);
                    childrenCountSlider.IsEnabled = false;
                    childrenCountSlider.IsVisible = false;
                    LabelChildren.IsVisible = false;
                    GridUserRole.IsVisible = false;
                    LabelChosenStop.IsVisible = false;

                    childrenCountSlider.IsVisible = false;
                    Gurdian.IsVisible = false;

                    LabelChosenRoute.IsVisible = true;
                    LabelBuses.IsVisible = true;

                    Father.Text = "Male";
                    Mother.Text = "Female";
                    LabelRole.Text = "Gender: ";

                    LabelRoutePicker.Text = "Select your bus...";
                    LabelChosenRoute.Text = "Route: ";

                    // Display data of the user
                    ParentName.Text = driver.Name;
                    ParentEmail.Text = driver.Email;
                    ParentPhone.Text = driver.PhoneNumber;
                    UserRolesPicker.SelectedItem = driver.user;
                    Mother.IsChecked = driver.IsFemale;
                    Father.IsChecked = driver.IsMale;

                    if (String.IsNullOrWhiteSpace(driver.Image) == false)
                    {
                        _image = driver.Image;
                        avatarProfile.ImageSource = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(driver.Image)));
                    }
                    if (driver.bus != null)
                    {
                        var userRoute = routes.Where(p => JsonConvert.SerializeObject(p.Object) == JsonConvert.SerializeObject(driver.bus.Route)).FirstOrDefault()?.Object;
                        string? Key = routes.FirstOrDefault(p => JsonConvert.SerializeObject(p.Object) == JsonConvert.SerializeObject(driver.bus.Route))?.Key;
                        if (String.IsNullOrWhiteSpace(Key) == false || userRoute != null)
                        {
                            if (driver.bus != null)
                                LabelBuses.Text = "Bus: " + driver.bus.BusNumber;
                            LabelChosenRoute.Text = "Route: " + Key;
                            _selectedRoute = userRoute;
                        }
                        else
                        {
                            await DisplayAlert("Error", "The route does not exist, contact administrators or set a new route...", "OK");
                            LabelBuses.Text = "Bus: " + "BUS NOT FOUND!";
                            LabelChosenRoute.Text = "Route: " + "ROUTE NOT FOUND!";
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Unsuccessful retrieval of user data", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Unsuccessful retrieval of user data at transferred email...", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else
            {
                await DisplayAlert("Error", "Unsuccessful transfer of email...", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        private class StopDisplayItem
        {
            public string? Display { get; set; }
            public Stop? Stops { get; set; }

            public override string ToString()
            {
                return Display ?? string.Empty; // Display the key in the picker
            }
        }

        private async Task<List<StopDisplayItem>> LoadStops(string rkey)
        {
            // Asynchronous loading of routes
            var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var stops = await firebase
                .Child($"Routes/{rkey}/Stops")
                .OnceAsync<Stop>(); // Fetching the routes from Firebase

            // Create a list to hold the route display items
            var stopDisplayItems = new List<StopDisplayItem>();

            // Loop through the fetched routes and use the keys (e.g., Route1, Route2) as display names
            foreach (var s in stops)
            {
                stopDisplayItems.Add(new StopDisplayItem
                {
                    Display = s.Key, // Use the key (e.g., Route1) as the display text
                    Stops = s.Object // The actual route object
                });
            }
            return stopDisplayItems;
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
                LabelChosenRoute.Text = "Chosen Route: " + selectedRouteItem.Display;
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
                        LabelChosenStop.Text = "Chosen Stop: " + _selectedStop?.Area;
                        // You can now use _selectedStop to know the user's stop on the route
                        Console.WriteLine($"Selected Stop: {_selectedStop?.Area} at Latitude: {_selectedStop?.Coordinates?.Latitude}, Longitude: {_selectedStop?.Coordinates?.Longitude}");
                    }
                }
            }
            else if (RoutePicker.SelectedItem is BusDisplayItem selectedBusItem)
            {
                LabelBuses.Text = "Bus: " + selectedBusItem.Display;
                var routes = await client
                .Child("Routes")
                .OnceAsync<Routes>(); // Fetching the routes from Firebase
                if (selectedBusItem.Buses != null && selectedBusItem.Buses.Route != null)
                {
                    string? Key = routes.FirstOrDefault(p => JsonConvert.SerializeObject(p.Object) == JsonConvert.SerializeObject(selectedBusItem.Buses.Route))?.Key;
                    LabelChosenRoute.Text = "Route: " + Key;
                    //_selectedRoute = selectedBusItem.Buses.Route;
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

        private async Task<List<BusDisplayItem>> LoadBuses()
        {
            // Asynchronous loading of buses
            var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var buses = await firebase
                .Child("Buses")
                .OnceAsync<Bus>(); // Fetching the Buses from Firebase

            // Create a list to hold the route display items
            var busDisplayItems = new List<BusDisplayItem>();

            // Loop through the fetched routes and use the keys (e.g., LXB123 GP, etc) as display names
            foreach (var bus in buses)
            {
                busDisplayItems.Add(new BusDisplayItem
                {
                    Display = bus.Object.BusNumber, // Use the key (e.g., BusNumber) as the display text
                    Buses = bus.Object // The actual route object
                });
            }
            return busDisplayItems; //Return a BusDisplayItem list with all relevant attributes
        }

        // Helper class to display route name and the route object
        private class BusDisplayItem
        {
            public string? Display { get; set; } // The key (e.g. "LXB123 GP")
            public Bus? Buses { get; set; } // The actual Bus object

            public override string ToString()
            {
                return Display ?? string.Empty; // Display the key in the picker
            }
        }

        private class ParentDisplayItem
        {
            public string? Display { get; set; }
            public Parent? Parents { get; set; }

            public override string ToString()
            {
                return Display ?? string.Empty; // Display the key
            }
        }

        private async Task<ParentDisplayItem> LoadParentKey(string email)
        {
            // Asynchronous loading of routes
            var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var parents = await firebase
                .Child("Parent")
                .OnceAsync<Parent>(); // Fetching the routes from Firebase

            // Create a list to hold the route display items
            var parentDisplayItem = new ParentDisplayItem();

            // Loop through the fetched routes and use the keys (e.g., Route1, Route2) as display names
            foreach (var par in parents)
            {
                if (par.Object.Email == email)
                {
                    parentDisplayItem = (new ParentDisplayItem
                    {
                        Display = par.Key, // Use the key (e.g., Route1) as the display text
                        Parents = par.Object // The actual route object
                    });
                }

            }
            return parentDisplayItem;
        }

        private class DriverDisplayItem
        {
            public string? Display { get; set; }
            public Driver? Drivers { get; set; }

            public override string ToString()
            {
                return Display ?? string.Empty; // Display the key
            }
        }

        private async Task<DriverDisplayItem> LoadDriverKey(string email)
        {
            // Asynchronous loading of routes
            var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var drivers = await firebase
                .Child("Driver")
                .OnceAsync<Driver>(); // Fetching the routes from Firebase

            // Create a list to hold the route display items
            var driverDisplayItem = new DriverDisplayItem();

            // Loop through the fetched routes and use the keys (e.g., Route1, Route2) as display names
            foreach (var dr in drivers)
            {
                if (dr.Object.Email == email)
                {
                    driverDisplayItem = (new DriverDisplayItem
                    {
                        Display = dr.Key, // Use the key (e.g., Route1) as the display text
                        Drivers = dr.Object // The actual route object
                    });
                }

            }
            return driverDisplayItem;
        }

        public void LoadUsers()
        {
            // Asynchronous loading of users
            var users = client.Child("Users").OnceAsync<Users>();
            Users = users.Result.Select(x => x.Object).ToList();
        }

        private async void LoadUserDetails()
        {
            var loggedInUserEmail = UserSession.LoggedInUserEmail;
            var parentList = await GetAllParents();

            ParentList.Clear();
            ChildrenList.Clear();
            DriverList.Clear();

            foreach (var parent in parentList)
            {
                ParentList.Add(parent);
                foreach (var child in parent.Children)
                {
                    ChildrenList.Add(child);
                }
            }
            var driverList = await GetDrivers();
            foreach (var driver in driverList)
            {
                DriverList.Add(driver);
            }

        }

        //LIST TO RETRIEVE ALL PARENTS
        private async Task<List<Parent>> GetAllParents()
        {

            return (await client
              .Child("Parent")
              .OnceAsync<Parent>()).Select(item => new Parent
              {
                  //ParentID = item.Object.ParentID,
                  Name = item.Object.Name,
                  //ParentType = item.Object.ParentType,
                  IsMother = item.Object.IsMother,
                  IsFather = item.Object.IsFather,
                  IsGurdian = item.Object.IsGurdian,
                  PhoneNumber = item.Object.PhoneNumber,
                  Email = item.Object.Email,
                  Password = item.Object.Password,
                  Route = item.Object.Route,
                  user = item.Object.user,
                  Children = item.Object.Children
              }).ToList();
        }
        // LIST TO RETRIEVE ALL DRIVERS
        private async Task<List<Driver>> GetDrivers()
        {

            return (await client
              .Child("Driver")
              .OnceAsync<Driver>()).Select(item => new Driver
              {
                  Name = item.Object.Name,
                  //ParentType = item.Object.ParentType,
                  IsMale = item.Object.IsMale,
                  IsFemale = item.Object.IsFemale,
                  PhoneNumber = item.Object.PhoneNumber,
                  Email = item.Object.Email,
                  Password = item.Object.Password,
                  user = item.Object.user,
                  bus = item.Object.bus,
              }).ToList();
        }
        // Generate and populate children grid and entries according to integer value
        private void GenerateChildrenGrid(int _count)
        {
            // Clear existing elements and definitions
            ChildrenGrid.Children.Clear();
            ChildrenGrid.RowDefinitions.Clear();
            ChildrenGrid.ColumnDefinitions.Clear();

            if (Convert.ToInt32(_count) is int count && count > 0)
            {
                // Define columns for Name/Grade and Gender
                ChildrenGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                ChildrenGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                // Define rows for each child
                for (int i = 0; i < Convert.ToInt32(childrenCountSlider.Value); i++)
                {
                    // Add rows for each child
                    ChildrenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Name
                    ChildrenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Grade
                    ChildrenGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Gender

                    // Create and set properties for child name entry
                    var childNameLabel = new Label { Text = $"Child {i + 1} Name" };
                    childNameLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    childNameLabel.VerticalOptions = LayoutOptions.Center;
                    var childNameEntry = new Entry { Placeholder = $"Enter Child {i + 1} Name", AutomationId = $"ChildName{i}" };
                    childNameEntry.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    childNameEntry.SetValue(Grid.RowProperty, i * 3); // Set row
                    childNameEntry.SetValue(Grid.ColumnProperty, 1); // Set column
                    childNameLabel.SetValue(Grid.RowProperty, i * 3); // Set row
                    childNameLabel.SetValue(Grid.ColumnProperty, 0); // Set column


                    // Create and set properties for child grade entry
                    var childGradeLabel = new Label { Text = $"Child {i + 1} Grade" };
                    childGradeLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    childGradeLabel.VerticalOptions = LayoutOptions.Center;
                    var childGradeEntry = new Entry { Placeholder = $"Enter Child {i + 1} Grade", AutomationId = $"ChildGrade{i}" };
                    childGradeEntry.SetValue(Grid.RowProperty, i * 3 + 1); // Set row
                    childGradeEntry.SetValue(Grid.ColumnProperty, 1); // Set column
                    childGradeLabel.SetValue(Grid.RowProperty, i * 3 + 1); // Set row
                    childGradeLabel.SetValue(Grid.ColumnProperty, 0); // Set column
                    childGradeEntry.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");

                    // Create and set properties for child gender layout
                    var genderLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
                    var maleRadioButton = new RadioButton { Content = "Male", GroupName = $"GenderGroup{i}", AutomationId = $"ChildGenderMale{i}" };
                    maleRadioButton.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    var femaleRadioButton = new RadioButton { Content = "Female", GroupName = $"GenderGroup{i}", AutomationId = $"ChildGenderFemale{i}" };
                    femaleRadioButton.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    genderLayout.Children.Add(maleRadioButton);
                    genderLayout.Children.Add(femaleRadioButton);
                    var genderLabel = new Label { Text = $"Child {i + 1} Gender" };
                    genderLabel.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#757575");
                    genderLabel.VerticalOptions = LayoutOptions.Center;
                    genderLayout.SetValue(Grid.RowProperty, i * 3 + 2); // Set row
                    genderLayout.SetValue(Grid.ColumnProperty, 1); // Set column
                    genderLabel.SetValue(Grid.RowProperty, i * 3 + 2); // Set row
                    genderLabel.SetValue(Grid.ColumnProperty, 0); // Set column

                    // Add the elements to the grid
                    ChildrenGrid.Children.Add(childNameLabel);
                    ChildrenGrid.Children.Add(childNameEntry);
                    ChildrenGrid.Children.Add(childGradeLabel);
                    ChildrenGrid.Children.Add(childGradeEntry);
                    ChildrenGrid.Children.Add(genderLabel);
                    ChildrenGrid.Children.Add(genderLayout);

                    if (i < ChildrenList.Count)
                    {
                        childNameEntry.Text = ChildrenList[i].Name;
                        childGradeEntry.Text = ChildrenList[i].Grade;
                        maleRadioButton.IsChecked = Convert.ToBoolean(ChildrenList[i].IsMale);
                        femaleRadioButton.IsChecked = Convert.ToBoolean(ChildrenList[i].IsFemale);
                    }
                }
            }
        }

        //Asynchronous method taking in sender email, subject and the body message to send email using the safetyriderteam email.
        private async void SendEmail(string senderEmail, string subject, string messageBody)
        {
            //Ceate a MimeMessage object within which the message data will be filled.
            MimeMessage message = new MimeMessage();
            //Add the sender information for the email message.
            message.From.Add(new MailboxAddress("SafetyRiderTeam", "safetyriderteam@gmail.com"));
            //Add the receiver information for the email message.
            message.To.Add(MailboxAddress.Parse(senderEmail));
            //Add the subject of the message
            message.Subject = subject;
            //Add the message body of the message
            message.Body = new TextPart("plain") { Text = messageBody };
            //Create the new SMTP client
            SmtpClient smtpClient = new SmtpClient();

            try
            {
                //Attempted connection to smtp server with SSL enabled
                smtpClient.Connect("smtp.gmail.com", 465, true);
                await smtpClient.AuthenticateAsync("safetyriderteam@gmail.com", "iacx gqdz bkea eijd");
                await smtpClient.SendAsync(message);
                //Details successfuly sent to email!
                await DisplayAlert("Success!", "Details have been sent to your email...", "Ok");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Connection Failed: \n" + ex, "Ok");
            }
            finally
            {
                //In any case... disconnect and displose of the client
                smtpClient.Disconnect(true);
                smtpClient.Dispose();
            }
        }

        // SELECT THE IMAGE AND UPLOAD IT TO TEMPORARY STORAGE AUTOMATICALLY
        private async Task<string> PickImageAsync()
        {
            var fileresult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Please select a self-portrait image: ",
                FileTypes = FilePickerFileType.Images
            });

            if (fileresult != null)
            {
                var stream = await fileresult.OpenReadAsync();

                var uploadedImagePath = await UploadLocalAsync(fileresult.FileName, stream);
                //avatarProfile.ImageSource = uploadedImagePath;
                Debug.WriteLine(uploadedImagePath);
                return uploadedImagePath;
            }
            else
            {
                await DisplayAlert("Error", "Cannot handle selected file...", "Ok");
                return String.Empty;
            }
        }

        // UPLOAD FILE TO TEMPRARY STORAGE
        private async Task<string> UploadLocalAsync(string fileName, Stream stream)
        {
            var localpath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using var fs = new FileStream(localpath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fs);
            //Debug.WriteLine(localpath);
            return localpath;
        }

        // RESIZE IMAGE AT INPUTTED PATH THEN CONVERT TO BASE64STRING
        private string ConvertImageToBase64(string localPath)
        {
            try
            {
                byte[] imageArray = System.IO.File.ReadAllBytes($"{localPath}");
                //RESIZING IMAGE TO 100*100
                using (MemoryStream ms = new MemoryStream(imageArray, 0, imageArray.Length))
                {
                    using (System.Drawing.Image img = System.Drawing.Image.FromStream(ms))
                    {
                        int h = 100;
                        int w = 100;

                        using (Bitmap b = new Bitmap(img, new System.Drawing.Size(w, h)))
                        {
                            using (MemoryStream ms2 = new MemoryStream())
                            {
                                b.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                                imageArray = ms2.ToArray();
                            }
                        }
                    }
                }

                string base64Image = Convert.ToBase64String(imageArray);
                return base64Image;
            }
            catch (Exception ex)
            {
                DisplayAlert("File Error...", ex.Message, "Error");
                return String.Empty;
            }
        }

        private void childrenCountSlider_ValueChangeEnd(object sender, EventArgs e)
        {
            GenerateChildrenGrid(Convert.ToInt32(childrenCountSlider.Value));
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
                entry.TextColor = Colors.DarkGray; // Reset color for valid email
            }
            LabelEmail.Text = "Email: " + e.NewTextValue;
        }
        private void ParentName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LabelName.Text = "Name: " + e.NewTextValue;
        }



        private async void buttonAvatarImage_Clicked(object sender, EventArgs e)
        {
            string localImagePath = await PickImageAsync();
            if (localImagePath != null)
            {
                avatarProfile.ImageSource = localImagePath;
                //Debug.WriteLine($"{localImagePath}");

                string base64String = ConvertImageToBase64(localImagePath);
                _image = base64String;
                //Debug.WriteLine(base64String);
                //await DisplayAlert("Test", base64String, "Ok");
            }
            //LabelChosenStop.Text = ConvertImageToBase64(localImagePath);
        }

        private async void OnAddParent(object sender, EventArgs e)
        {
            if (UserRolesPicker.SelectedItem is Users user && email != null)
            {
                //Disable the add parent button to prevent double clicking and double entering
                AddParent.IsEnabled = false;
                if (user.UserRole == "Parent") // && (RoutePicker.SelectedItem is RouteDisplayItem selectedRouteItem)
                {
                    //var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
                    var buses = await client.Child("Buses")
                        .OnceAsync<Bus>();
                    var bus = buses.Where(p => JsonConvert.SerializeObject(p.Object.Route) == JsonConvert.SerializeObject(_selectedRoute)).FirstOrDefault()?.Object;
                    var parents = await client.Child("Parent").OnceAsync<Parent>();
                    var parentInfo = parents.FirstOrDefault(p => p.Object.Email == email)?.Object;
                    //Prevent saving of new parent user if bus object is null.
                    if (bus != null)
                    {
                        if (parentInfo != null)
                        {
                            var parent = new Parent
                            {
                                // Create a Parent object with all required fields set
                                Name = ParentName.Text,
                                IsMother = Mother.IsChecked,
                                IsFather = Father.IsChecked,
                                IsGurdian = Gurdian.IsChecked,
                                PhoneNumber = ParentPhone.Text,
                                Email = parentInfo.Email,
                                Password = parentInfo.Password, // Store the hashed password
                                Image = _image,
                                user = user,
                                Route = _selectedRoute, // Set the route here
                                Children = new List<Children>()
                            };
                            if (childrenCountSlider.Value > 0)
                            {
                                // Add children entries
                                for (int i = 0; i < ChildrenGrid.RowDefinitions.Count; i += 3)
                                {
                                    var childNameEntry = ChildrenGrid.Children.OfType<Entry>().FirstOrDefault(e => (int)e.GetValue(Grid.RowProperty) == i && (int)e.GetValue(Grid.ColumnProperty) == 1);
                                    var childGradeEntry = ChildrenGrid.Children.OfType<Entry>().FirstOrDefault(e => (int)e.GetValue(Grid.RowProperty) == i + 1 && (int)e.GetValue(Grid.ColumnProperty) == 1);
                                    var childGenderLayout = ChildrenGrid.Children.OfType<StackLayout>().FirstOrDefault(s => (int)s.GetValue(Grid.RowProperty) == i + 2 && (int)s.GetValue(Grid.ColumnProperty) == 1);

                                    // Determine the selected gender
                                    bool? isFemale = null;
                                    bool? isMale = null;

                                    if (childGenderLayout != null)
                                    {
                                        var maleRadioButton = childGenderLayout.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.Content.ToString() == "Male" && rb.IsChecked);
                                        var femaleRadioButton = childGenderLayout.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.Content.ToString() == "Female" && rb.IsChecked);

                                        if (maleRadioButton != null) isMale = true;
                                        if (femaleRadioButton != null) isFemale = true;
                                    }
                                    //POSSIBLE ISSUE AS A CHILD IS ADDED IN ALL CASES... HOWEVER CHILDREN CAN SIMPLY BE EDITED AND OVERWRITTEN IN THE EDIT USERS PAGE...
                                    parent.Children.Add(new Children
                                    {
                                        Name = childNameEntry?.Text,
                                        Grade = childGradeEntry?.Text,
                                        IsFemale = isFemale,
                                        IsMale = isMale,
                                        BusNumber = bus.BusNumber,
                                    });
                                }
                            }
                            else
                            {
                                await DisplayAlert("NOTE!!!", "No children have been added!", "OK");
                            }

                            var parentKey = new ParentDisplayItem();
                            // Loop through parents from the firebase and get key of the user
                            foreach (var par in parents)
                            {
                                if (par.Object.Email == parentInfo.Email)
                                {
                                    parentKey = (new ParentDisplayItem
                                    {
                                        Display = par.Key, // Use the key (e.g., Route1) as the display text
                                        Parents = par.Object // The actual route object
                                    });
                                }
                            }
                            // Save parent and children to Firebase
                            try
                            {
                                await client.Child("Parent/" + parentKey.Display)
                            .PatchAsync(parent);
                                await DisplayAlert("Success!", "User successfully edited...", "OK");
                                await Shell.Current.GoToAsync("..");
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Failure!", "Edit failed. Check your connection or data input.", "OK");
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "No user found at this email...", "OK");
                        }
                        //SendEmail(ParentEmail.Text, "New Parent! Welcome to Safety Rider, " + parent.Name + "!", "Account Email: " + parent.Email + "\nAccount Password: " + ParentPassword.Text + "\nPhone Number: " + parent.PhoneNumber);
                    }
                    else
                    {   //Case for when bus object is null
                        await DisplayAlert("Error", "No supporting bus found for the selected route...", "OK");
                        AddParent.IsEnabled = true;
                    }
                }
                else if (user.UserRole == "Driver")
                {
                    // ADD CODE TO MAKE SURE THAT THE DRIVER IS NOT BEING ASSIGNED TO A BUS THAT HAS A DRIVER ALREADY

                    var driverKey = await LoadDriverKey(email);
                    var drivers = await client.Child("Driver").OnceAsync<Driver>();
                    var driverInfo = drivers.FirstOrDefault(p => p.Object.Email == email)?.Object;
                    // declare _bus depending on either _selectedroute or selectedBusItem is present
                    if (driverInfo != null)
                    {
                        if (RoutePicker.SelectedItem is BusDisplayItem selectedBusItem)
                        {
                            var _checkBus = drivers.FirstOrDefault(p => p.Object?.bus?.BusNumber == selectedBusItem.Buses?.BusNumber && p.Object?.Email != email)?.Object;
                            if (_checkBus == null)
                            {
                                var driver = new Driver
                                {
                                    Name = ParentName.Text,
                                    IsFemale = Mother.IsChecked,
                                    IsMale = Father.IsChecked,
                                    Email = email,
                                    Password = driverInfo.Password,
                                    PhoneNumber = ParentPhone.Text,
                                    Image = _image,
                                    user = user,
                                    bus = selectedBusItem.Buses,
                                };
                                try
                                {
                                    // Save driver and bus to Firebase
                                    await client.Child("Driver/" + driverKey?.Display)
                                        .PatchAsync(driver);
                                    await DisplayAlert("Success!", "User successfully edited...", "OK");
                                    await Shell.Current.GoToAsync("..");
                                    //SendEmail(ParentEmail.Text, "New Driver! Welcome to Safety Rider, " + driver.Name + "!", "Account Email: " + driver.Email + "\nAccount Password: " + ParentPassword.Text + "\nPhone Number: " + driver.PhoneNumber
                                    //    + "\n\n\nDISCLAIMER: If this was NOT you, please contact the admins at SafetyRider.");
                                }
                                catch (Exception ex)
                                {
                                    await DisplayAlert("Failure!", "Edit failed. Check your connection or data input.", "OK");
                                    Debug.WriteLine(ex.Message);
                                }
                            }
                            else
                            {
                                await DisplayAlert("Error", "Selected bus is already in use...", "OK");
                                AddParent.IsEnabled = true;
                            }
                        }
                        else if (_selectedRoute is Routes _route)
                        {                           
                            var routes = await client
                                               .Child("Routes")
                                               .OnceAsync<Routes>(); // Fetching the routes from Firebase
                            if (_route != null)
                            {
                                var buses = await client.Child("Buses")
                                .OnceAsync<Bus>();
                                var bus = buses.Where(p => JsonConvert.SerializeObject(p.Object.Route) == JsonConvert.SerializeObject(_route)).FirstOrDefault()?.Object;
                                var _checkBus = drivers.FirstOrDefault(p => p.Object?.bus?.BusNumber == bus?.BusNumber && p.Object?.Email != email)?.Object;
                                if (_checkBus == null)
                                {
                                    var driver = new Driver
                                    {
                                        Name = ParentName.Text,
                                        IsFemale = Mother.IsChecked,
                                        IsMale = Father.IsChecked,
                                        Email = email,
                                        Image = _image,
                                        Password = driverInfo.Password,
                                        PhoneNumber = ParentPhone.Text,
                                        user = user,
                                        bus = bus,
                                    };
                                    try
                                    {
                                        // Save driver and bus to Firebase
                                        await client.Child("Driver/" + driverKey?.Display)
                                            .PatchAsync(driver);
                                        await DisplayAlert("Success!", "User successfully edited...", "OK");
                                        await Shell.Current.GoToAsync("..");
                                        //SendEmail(ParentEmail.Text, "New Driver! Welcome to Safety Rider, " + driver.Name + "!", "Account Email: " + driver.Email + "\nAccount Password: " + ParentPassword.Text + "\nPhone Number: " + driver.PhoneNumber
                                        //    + "\n\n\nDISCLAIMER: If this was NOT you, please contact the admins at SafetyRider.");
                                    }
                                    catch (Exception ex)
                                    {
                                        await DisplayAlert("Failure!", "Edit failed. Check your connection or data input.", "OK");
                                        Debug.WriteLine(ex.Message);
                                        AddParent.IsEnabled = true;
                                    }
                                }
                                else
                                {
                                    await DisplayAlert("Error", "Selected bus is already in use...", "OK");
                                    AddParent.IsEnabled = true;
                                }
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Route not found! Please try selecting another route...", "OK");
                            AddParent.IsEnabled = true;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error fetching driver details...", "OK");
                        AddParent.IsEnabled = true;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Unexpected error. Please contact administrators...", "OK");
                    AddParent.IsEnabled = true;
                }
            }
            else
            {
                // Handle the case where no user or route is selected
                await DisplayAlert("Error", "Please select a user role and a route.", "OK");
                AddParent.IsEnabled = true;
                //await Shell.Current.GoToAsync("..");
            }
        }

        private async void ClearParent_Clicked(object sender, EventArgs e)
        {
            //REFRESHING
            // Get current page
            var page = Navigation.NavigationStack.LastOrDefault();

            // Load new page
            if (email != null)
            {
                await Shell.Current.GoToAsync($"{nameof(EditUsersPage)}?email={Uri.EscapeDataString(email)}", false);
            }
            else
            {
                await Shell.Current.GoToAsync(nameof(EditUsersPage), false);
            }
            // Remove old page
            Navigation.RemovePage(page);
        }

        private async void DeleteParent_Clicked(object sender, EventArgs e)
        {
            bool check = await DisplayAlert("Confirm", $"Are you sure you want to Delete {email}? All changes are permanent...", "Yes", "No");
            if (check)
            {
                var parents = await client.Child("Parent").OnceAsync<Parent>();
                var parentInfo = parents.FirstOrDefault(p => p.Object.Email == email)?.Object;
                
                var drivers = await client.Child("Driver").OnceAsync<Driver>();
                var driverInfo = drivers.FirstOrDefault(p => p.Object.Email == email)?.Object;
                
                // Loop through parents from the firebase and get key of the user
                if (parentInfo != null && driverInfo == null)
                {
                    if (String.IsNullOrWhiteSpace(parentInfo.Email) == false)
                    {
                        var parentKey = new ParentDisplayItem();
                        foreach (var par in parents)
                        {
                            if (par.Object.Email == parentInfo.Email)
                            {
                                parentKey = (new ParentDisplayItem
                                {
                                    Display = par.Key,
                                    Parents = par.Object
                                });
                            }
                        }
                        await client.Child("Parent/" + parentKey.Display)
                        .DeleteAsync();
                        await DisplayAlert("User deleted.", $"It's a shame to see {parentInfo.Name} go...", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Contact admins...", "OK");
                    }
                }
                else if (driverInfo != null && parentInfo == null)
                {
                    if (String.IsNullOrWhiteSpace(driverInfo.Email) == false)
                    {
                        var driverKey = new DriverDisplayItem();
                        foreach (var dr in drivers)
                        {
                            if (dr.Object.Email == driverInfo.Email)
                            {
                                driverKey = (new DriverDisplayItem
                                {
                                    Display = dr.Key,
                                    Drivers = dr.Object
                                });
                            }
                        }
                        await client.Child("Driver/" + driverKey.Display)
                        .DeleteAsync();
                        await DisplayAlert("User deleted.", $"It's a shame to see {driverInfo.Name} go...", "OK");
                        await Shell.Current.GoToAsync("..");
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Contact admins...", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Contact admins...", "OK");
                }
            }
            else
            {
                return;
            }
        }

        private async void SfRadialMenuItem_BackItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
        private async void SfRadialMenuItem_RouteItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(AddRoutesPage)}");
        }
        private async void SfRadialMenuItem_MainItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(MainPage)}");
        }
        private async void SfRadialMenuItem_ItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(NotificationsPage)}");
        }
        private async void SfRadialMenuItem_DashboardItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
        }
    }
}