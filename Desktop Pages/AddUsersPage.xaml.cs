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
using Microsoft.Maui.Storage;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Platform;
using System.Drawing;
using Newtonsoft.Json;
using Microsoft.Maui.ApplicationModel.Communication;

namespace AdminApp;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class AddUsersPage : ContentPage
{
    private Routes? _selectedRoute; // Nullable Routes
    //private string? _selectedRouteDisplayText; // Nullable string
    private bool _validEmailCheck;
    private string _image = String.Empty;

    private FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
    private List<Routes>? Routes { get; set; } // List of routes
    private Stop? _selectedStop;
    public List<Users>? Users { get; set; }
    private const int MaxPhoneNumberLength = 10;
    //public int childrenCount;
    //SfPopup popupUserRoles = new SfPopup();
    public AddUsersPage()
    {
        InitializeComponent();
        LoadUsers();
        LoadRoutes();
        BindingContext = this;
        //ChildrenCountPicker.SelectedIndexChanged += OnChildrenCountChanged;
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
            LabelBuses.Text = "Chosen Bus: " + selectedBusItem.Display;
            var routes = await client
                .Child("Routes")
                .OnceAsync<Routes>(); // Fetching the routes from Firebase
            if (selectedBusItem.Buses != null && selectedBusItem.Buses.Route != null)
            {
                string? Key = routes.FirstOrDefault(p => JsonConvert.SerializeObject(p.Object) == JsonConvert.SerializeObject(selectedBusItem.Buses.Route))?.Key;
                LabelChosenRoute.Text = "Chosen Route: " + Key;
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

    public void LoadUsers()
    {
        // Asynchronous loading of users
        var users = client.Child("Users").OnceAsync<Users>();
        Users = users.Result.Select(x => x.Object).ToList();
    }
    // Example for replacing the content in case of readonly collection
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
            _validEmailCheck = false;
        }
        else
        {
            entry.TextColor = Colors.Black; // Reset color for valid email
            _validEmailCheck = true;
        }
        LabelEmail.Text = "Email: " + e.NewTextValue;
    }
    private void ParentName_TextChanged(object sender, TextChangedEventArgs e)
    {
        LabelName.Text = "Name: " + e.NewTextValue;
    }
    private async void UserRolesPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Show the appropriate sections based on the selected role
        if (UserRolesPicker.SelectedItem is Users selectedUser)
        {
            if (selectedUser.UserRole == "Parent")
            {
                ParentStackLayout.IsVisible = true;
                AddParent.IsVisible = true;
                GridUserRole.IsVisible = false;
                PreviewStackLayout.IsVisible = true;
                RouteStackLayout.IsVisible = true;
                buttonAvatarImage.IsVisible = true;
            }
            else
            {
                ParentStackLayout.IsVisible = true;
                AddParent.IsVisible = true;
                PreviewStackLayout.IsVisible = true;
                //LabelBuses.IsVisible = true;
                RouteStackLayout.IsVisible = true;
                buttonAvatarImage.IsVisible = true;
                LabelBuses.IsVisible = true;
                LabelChildren.IsVisible = false;
                GridUserRole.IsVisible = false;
                LabelChosenStop.IsVisible = false;

                childrenCountSlider.IsVisible = false;
                Gurdian.IsVisible = false;

                Father.Text = "Male";
                Mother.Text = "Female";
                LabelRole.Text = "Gender: ";

                LabelRoutePicker.Text = "Select your bus...";
                LabelChosenRoute.Text = "Chosen Route: ";
                LabelBuses.Text = "Chosen Bus: ";

                RoutePicker.ItemsSource = await LoadBuses();
            }
        }

        else
        {
            // If no role is selected, hide the other fields
            ParentStackLayout.IsVisible = false;
        }
    }

    private void childrenCountSlider_ValueChangeEnd(object sender, EventArgs e)
    {
        Debug.WriteLine(childrenCountSlider.Value.ToString());
        // Clear existing elements and definitions
        ChildrenGrid.Children.Clear();
        ChildrenGrid.RowDefinitions.Clear();
        ChildrenGrid.ColumnDefinitions.Clear();

        if (Convert.ToInt32(childrenCountSlider.Value) is int count && count > 0)
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
        message.Body = new TextPart("plain") { Text  = messageBody };
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
            await DisplayAlert("Error","Cannot handle selected file...","Ok");
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

    private async void ClearParent_Clicked(object sender, EventArgs e)
    {
        //REFRESHING
        // Get current page
        var page = Navigation.NavigationStack.LastOrDefault();

        // Load new page
        await Shell.Current.GoToAsync(nameof(AddUsersPage), false);

        // Remove old page
        Navigation.RemovePage(page);
    }

    private async void OnAddParent(object sender, EventArgs e)
    {        
        if (UserRolesPicker.SelectedItem is Users user)
        {
            //Disable the add parent button to prevent double clicking and double entering
            AddParent.IsEnabled = false;
            if (_validEmailCheck && String.IsNullOrWhiteSpace(ParentPassword.Text) == false)
            {
                // Hash the password before saving it
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(ParentPassword.Text);
                var _checkParents = await client.Child("Parent").OnceAsync<Parent>();
                var _checkParent = _checkParents.FirstOrDefault(p => p.Object.Email == ParentEmail.Text)?.Object;

                var _checkDrivers = await client.Child("Driver").OnceAsync<Driver>();
                var _checkDriver = _checkDrivers.FirstOrDefault(p => p.Object.Email == ParentEmail.Text)?.Object;
                //Check if the entered email is present in any of the driver or parent tables
                if(_checkDriver == null && _checkParent == null)
                {
                    //Determine the type of user being entered
                    if (user.UserRole == "Parent" && RoutePicker.SelectedItem is RouteDisplayItem selectedRouteItem)
                    {
                        var firebase = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
                        var buses = await client.Child("Buses")
                            .OnceAsync<Bus>();
                        var bus = buses.Where(p => JsonConvert.SerializeObject(p.Object.Route) == JsonConvert.SerializeObject(selectedRouteItem.Route)).FirstOrDefault()?.Object;
                        //Prevent saving of new parent user if bus object is null.
                        if (bus != null)
                        {
                            var parent = new Parent
                            {
                                // Create a Parent object with all required fields set
                                Name = ParentName.Text,
                                IsMother = Mother.IsChecked,
                                IsFather = Father.IsChecked,
                                IsGurdian = Gurdian.IsChecked,
                                PhoneNumber = ParentPhone.Text,
                                Email = ParentEmail.Text,
                                Password = hashedPassword, // Store the hashed password
                                Image = _image,
                                user = user,
                                Route = selectedRouteItem.Route, // Set the route here
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

                            // Save parent and children to Firebase
                            await client.Child("Parent").PostAsync(parent);
                            await DisplayAlert("Success!", "User successfully added...", "OK");
                            await Shell.Current.GoToAsync("..");
                            SendEmail(parent.Email, "New Parent! Welcome to Safety Rider, " + parent.Name + "!", "Account Email: " + parent.Email + "\nAccount Password: " + ParentPassword.Text + "\nPhone Number: " + parent.PhoneNumber);
                        }
                        else
                        {   //Case for when bus object is null
                            await DisplayAlert("Error", "No supporting bus found for the selected route...", "OK");
                            AddParent.IsEnabled = true;
                        }
                    }
                    else if (user.UserRole == "Driver" && RoutePicker.SelectedItem is BusDisplayItem selectedBusItem)
                    {
                        var drivers = await client.Child("Driver").OnceAsync<Driver>();
                        var _checkBus = drivers.FirstOrDefault(p => p.Object?.bus?.BusNumber == selectedBusItem?.Buses?.BusNumber)?.Object;
                        if (_checkBus == null)
                        {
                            var driver = new Driver
                            {
                                Name = ParentName.Text,
                                IsFemale = Mother.IsChecked,
                                IsMale = Father.IsChecked,
                                PhoneNumber = ParentPhone.Text,
                                Email = ParentEmail.Text,
                                Image = _image,
                                Password = hashedPassword, // Store the hashed password
                                user = user,
                                bus = selectedBusItem.Buses,
                            };
                            // Save driver and bus to Firebase
                            await client.Child("Driver").PostAsync(driver);
                            await Shell.Current.GoToAsync("..");
                            SendEmail(driver.Email, "New Driver! Welcome to Safety Rider, " + driver.Name + "!", "Account Email: " + driver.Email + "\nAccount Password: " + ParentPassword.Text + "\nPhone Number: " + driver.PhoneNumber
                                + "\n\n\nDISCLAIMER: If this was NOT you, please contact the admins at SafetyRider.");
                        }
                        else
                        {
                            await DisplayAlert("Error", "Selected bus is already in use...", "OK");
                            AddParent.IsEnabled = true;
                        }                       
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please select a route.", "OK");
                        AddParent.IsEnabled = true;
                    }
                }
                else
                {
                    //Case for when duplicate email is found
                    await DisplayAlert("Error", "Email already belongs to another user...", "OK");
                    AddParent.IsEnabled = true;
                }                
            }
            else
            {
                // Handle the case where no email or password is entered
                await DisplayAlert("Error", "Please enter a valid email and/or valid password...", "OK");
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

    private async void SfRadialMenuItem_ItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(NotificationsPage)}");
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
    private async void SfRadialMenuItem_DashboardItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
    }
}

