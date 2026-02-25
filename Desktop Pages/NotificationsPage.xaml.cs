using Firebase.Database;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Util;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Maui.Controls;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
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
using Grpc.Core;
using System.Reactive.Subjects;
using Microsoft.Maui.Storage;
using System.Text;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Platform;
using System.Drawing;

namespace AdminApp;

public partial class NotificationsPage : ContentPage
{
    FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");    
    public ObservableCollection<NotificationKey> NotificationsList { get; set; } = new ObservableCollection<NotificationKey>();
    public class CollectionViewItems
    {
        public string? Type { get; set; }
        public string? Image { get; set; }
    }
    public List<CollectionViewItems> getItems()
    {
        return new List<CollectionViewItems>
            {
                new CollectionViewItems {Type = "All", Image = "cvall.png"},
                new CollectionViewItems {Type = "Drivers", Image = "cvdriver.png"},
                new CollectionViewItems {Type = "Parents", Image = "cvparent.png"}
            };
    }
    public List<Notifications> getTestNotifications()
    {
        return new List<Notifications>
            {
                new Notifications {Title = "Accident: ", Description = "Delays due to accident on the M1... Please excuse the inconvenience, and contact the administrators at Safe S School for further clarification...", IsVisibleDriver = true, IsVisibleParent = true, TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                new Notifications {Title = "Accident: ", Description = "Delays due to accident on the M1... Please excuse the inconvenience, and contact the administrators at Safe S School for further clarification...", IsVisibleDriver = true, IsVisibleParent = true, TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                new Notifications {Title = "Accident: ", Description = "Delays due to accident on the M1... Please excuse the inconvenience, and contact the administrators at Safe S School for further clarification...", IsVisibleDriver = true, IsVisibleParent = true, TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                new Notifications {Title = "Accident: ", Description = "Delays due to accident on the M1... Please excuse the inconvenience, and contact the administrators at Safe S School for further clarification...", IsVisibleDriver = true, IsVisibleParent = true, TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
            };
    }
    public class NotificationKey : Notifications
    {
        public string? Key { get; set; }
    }
    public bool _isVisibleDriver = false;
    public bool _isVisibleParent = false;
    public bool _isEditMode = false;
    public string _editKey = String.Empty;
    public NotificationsPage()
    {
        InitializeComponent();
        BindingContext = this;
        collectionViewTypeItems.ItemsSource = getItems();        
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        loadNotifications();        
    }
    public async void loadNotifications()
    {
        var notificationsList = await getNotifications();
        var notificationsListOrdered = notificationsList.OrderByDescending(x => DateTime.ParseExact(x.TimeStamp ?? "01/01/2024 00:00:00", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        foreach (var notification in notificationsListOrdered)
        {
            NotificationsList.Add(notification);
        }

        collectionViewAllToday.ItemsSource = sortTimeNotifications(0, 1);
        collectionViewAllLastSevenDays.ItemsSource = sortTimeNotifications(0, 7);
        collectionViewAllLastThirtyOneDays.ItemsSource = sortTimeNotifications(0, 31);
        collectionViewAllLastYear.ItemsSource = sortTimeNotifications(31, 365);
        collectionViewAllDrivers.ItemsSource = sortTypeNotifications(true, false);
        collectionViewAllParents.ItemsSource = sortTypeNotifications(false, true);
    }
    public async Task<List<NotificationKey>> getNotifications()
    {
        return (await client
              .Child("Notifications")
              .OnceAsync<Notifications>()).Select(item => new NotificationKey
              {
                  Title = item.Object.Title,
                  Description = item.Object.Description,
                  IsVisibleDriver = item.Object.IsVisibleDriver,
                  IsVisibleParent = item.Object.IsVisibleParent,
                  TimeStamp = item.Object.TimeStamp,
                  Key = item.Key,
              }).ToList();
    }
    public List<Notifications> sortTimeNotifications(int daysstart, int daysend)
    {
        List<Notifications> sortedList = new List<Notifications>();
        foreach (var notification in NotificationsList)
        {
            if (notification.TimeStamp != null)
            {
                DateTime dateTimeStamp = DateTime.ParseExact(notification.TimeStamp, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                int timePeriod = Convert.ToInt32((DateTime.Now - dateTimeStamp).TotalDays);
                if (daysstart <= timePeriod && timePeriod <= daysend)
                {
                    sortedList.Add(notification);
                }
            }
        }
        return sortedList;
    }
    public List<Notifications> sortTypeNotifications(bool __isVisibleDriver, bool __isVisibleParent)
    {
        List<Notifications> sortedList = new List<Notifications>();
            foreach (var notification in NotificationsList)
            {
                if (__isVisibleDriver == notification.IsVisibleDriver && __isVisibleParent == notification.IsVisibleParent || (notification.IsVisibleDriver == true && notification.IsVisibleParent == true))
                {
                    sortedList.Add(notification);
                }
            }
        return sortedList;
    }

    private void collectionViewTypeItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CollectionViewItems selectedType = (CollectionViewItems)collectionViewTypeItems.SelectedItem;
        if (selectedType.Type == "All")
        {
            //await DisplayAlert("Test Case: ", "All selected", "OK");
            _isVisibleDriver = true;
            _isVisibleParent = true;
        }
        else if (selectedType.Type == "Drivers")
        {
            //await DisplayAlert("Test Case: ", "Drivers selected", "OK");
            _isVisibleDriver = true;
            _isVisibleParent = false;
        }
        else
        {
            //await DisplayAlert("Test Case: ", "Parents selected", "OK");
            _isVisibleDriver = false;
            _isVisibleParent = true;
        }
    }
    private async void CancelNotification_Clicked(object sender, EventArgs e)
    {
        //REFRESHING
        // Get current page
        var page = Navigation.NavigationStack.LastOrDefault();

        // Load new page
        await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

        // Remove old page
        Navigation.RemovePage(page);
    }

    private async void AddNotification_Clicked(object sender, EventArgs e)
    {
        AddNotification.IsEnabled = false;
        if (_isVisibleDriver == true || _isVisibleParent == true)
        {
            if (String.IsNullOrWhiteSpace(editorTitle.Text) == false && String.IsNullOrWhiteSpace(editorDescription.Text) == false)
            {
                var notification = new Notifications
                {
                    Title = editorTitle.Text,
                    Description = editorDescription.Text,
                    IsVisibleDriver = _isVisibleDriver,
                    IsVisibleParent = _isVisibleParent,
                    TimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                };
                // Save Notifications to Firebase
                if (_isEditMode == false)
                {
                    await client.Child("Notifications").PostAsync(notification);
                    await DisplayAlert("Success!", "Notification successfully added...", "OK");
                }
                else if (_isEditMode == true && String.IsNullOrWhiteSpace(_editKey) == false)
                {
                    notification.Description = notification.Description + "(Edited.)";
                    await client.Child("Notifications/" + _editKey)
                        .PatchAsync(notification);
                    await DisplayAlert("Success!", "Notification successfully edited...", "OK");
                }
                //AddNotification.IsEnabled = true;

                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            else
            {
                await DisplayAlert("Error: ", "Please make sure you have filled both Title and Description...", "OK");
                AddNotification.IsEnabled = true;
            }
        }
        else
        {
            {
                await DisplayAlert("Error: ", "Please select which user types may be able to view this notification...", "OK");
                AddNotification.IsEnabled = true;
            }
        }
    }   

    private async void collectionViewAllToday_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllToday.SelectedItem;
        _editKey = String.Empty;
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }

    private async void collectionViewAllLastSevenDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllLastSevenDays.SelectedItem;
        _editKey = String.Empty;
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }

    private async void collectionViewAllLastThirtyOneDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllLastThirtyOneDays.SelectedItem;
        _editKey = String.Empty;
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");                   
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }

    private async void collectionViewAllLastYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllLastYear.SelectedItem;
        _editKey = String.Empty;
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }

    private async void collectionViewAllParents_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllParents.SelectedItem;
        _editKey = String.Empty;
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }

    private async void collectionViewAllDrivers_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        NotificationKey selectedItem = (NotificationKey)collectionViewAllDrivers.SelectedItem;
        _editKey = String.Empty; 
        _isEditMode = false;
        var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
        if (selectedItem != null)
        {
            if (selectedOption == "Edit" && selectedItem.Key != null)
            {
                //await DisplayAlert("Test Case: ", "Edit", "OK");
                editorTitle.Text = selectedItem.Title;
                editorDescription.Text = selectedItem.Description;
                _editKey = selectedItem.Key;
                _isEditMode = true;
                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                return;
            }
            else if (selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this notification? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    await client.Child("Notifications/" + selectedItem.Key)
                        .DeleteAsync();
                    await DisplayAlert("Notification Deleted.", "The notification has successfully been removed.", "OK");
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(NotificationsPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            return;
        }
        //await DisplayAlert("Test Case: ", selectedItem.Key, "OK");
        return;
    }
    private async void SfRadialMenuItem_BackItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
    private async void SfRadialMenuItem_NotificationItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await DisplayAlert("Error:", "You are already on the selected page...", "OK");
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
        await Shell.Current.GoToAsync($"{nameof(MainPage)}");
    }
    private async void SfRadialMenuItem_DashboardItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
    }
}