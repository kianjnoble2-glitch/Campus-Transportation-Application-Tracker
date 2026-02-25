using Firebase.Database;
using Kats;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Kats
{
    public partial class NotificationsPage : ContentPage
    {
        public ObservableCollection<Notification> Notifications { get; set; }
        private readonly FirebaseClient _firebaseClient;

        public NotificationsPage()
        {
            InitializeComponent();
            Notifications = new ObservableCollection<Notification>();
            NotificationsCollectionView.ItemsSource = Notifications;

            // Initialize Firebase client
            _firebaseClient = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");

            LoadNotifications();
        }

        private async void LoadNotifications()
        {
            try
            {
                // Fetch notifications from Firebase
                var notifications = await GetNotificationsFromFirebase();

                // Filter notifications for parents
                var driverNotifications = notifications.Where(n => (bool)n.IsVisibleDriver);

                // Sort notifications by timestamp (latest first)
                var sortedNotifications = driverNotifications
                    .OrderByDescending(n => ParseDateTime(n.TimeStamp))
                    .ToList();

                // Populate the ObservableCollection
                Notifications.Clear();
                foreach (var notification in sortedNotifications)
                {
                    Notifications.Add(notification);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors)
                await DisplayAlert("Error", $"Failed to load notifications: {ex.Message}", "OK");
            }
        }

        private async Task<IEnumerable<Notification>> GetNotificationsFromFirebase()
        {
            var notifications = new List<Notification>();
            var snapshot = await _firebaseClient
                .Child("Notifications")
                .OnceAsync<Notification>();

            foreach (var item in snapshot)
            {
                var notification = item.Object;
                notifications.Add(notification);
            }

            return notifications;
        }

        private DateTime ParseDateTime(string? timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                // Handle the case where the timestamp is null or empty
                return DateTime.MinValue;
            }

            // Assuming the timestamp format is "dd/MM/yyyy HH:mm:ss"
            if (DateTime.TryParseExact(timestamp, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                return dateTime;
            }

            // Fallback to a default value or handle the parsing error
            return DateTime.MinValue;
        }

        private async void BacktoMain(object sender, EventArgs e)
        {
            await this.FadeTo(0, 500); // Fade out over 500 milliseconds

            // Navigate back to ParentMainPage with no animation
            await Navigation.PopAsync(animated: false);

            // Optionally,apply a fade-in effect to the ParentMainPage
            var currentPage = Navigation.NavigationStack.LastOrDefault();
            if (currentPage != null)
            {
                await currentPage.FadeTo(1, 500); // Fade in the new page
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
    }
}
public class Notification
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? TimeStamp { get; set; }
    public bool? IsVisibleDriver { get; set; }
    public bool? IsVisibleParent { get; set; }
}
