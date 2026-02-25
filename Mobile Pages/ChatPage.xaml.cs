using Firebase.Database;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Firebase.Database.Query;

namespace Kats
{
    public partial class ChatPage : ContentPage
    {
        FirebaseClient client = new("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        private string? parentEmail = "jeremybutton@gmail.com";
        private string? driverEmail;
        private string? parentName;

        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();
        public ChatPage() { }
        public ChatPage(string _name, string _email)
        {
            InitializeComponent();
            this.parentName = _name;
            this.parentEmail = _email;
            driverEmail = UserSession.LoggedInUserEmail;
            BindingContext = this;

            LoadChatMessages();
            SubscribeToChatUpdates();
        }

        private async void LoadChatMessages()
        {
            // Fetch all chat messages from Firebase
            var allMessages = await client.Child("chats").OnceAsync<ChatMessage>();

            // Clear existing messages
            ChatMessages.Clear();

            // Prepare a list to hold all messages
            var messagesList = new List<ChatMessage>();

            // Loop through each message and determine where to display it
            foreach (var message in allMessages)
            {
                var senderEmail = message.Object.Sender;

                // Set IsSentByCurrentUser based on the sender
                message.Object.IsSentByCurrentUser = senderEmail == driverEmail; // Assuming parent is the user sending messages

                // Get sender name
                message.Object.SenderName = await GetUserNameByEmail(senderEmail);
                message.Object.Timestamp = message.Object.Timestamp.ToLocalTime(); // Convert to local time

                messagesList.Add(message.Object);
            }

            // Sort messages based on Timestamp to ensure chronological order
            messagesList = messagesList.OrderBy(m => m.Timestamp).ToList();

            // Add sorted messages to ChatMessages collection
            foreach (var message in messagesList)
            {
                ChatMessages.Add(message);
            }

            if (ChatMessages.Count > 0)
            {
                RecentChatsView.ScrollTo(ChatMessages.Last(), animate: true);
            }
        }


        private async void SubscribeToChatUpdates()
        {
            client.Child("chats").AsObservable<ChatMessage>().Subscribe(d =>
            {
                if (d.Object.Receiver == parentEmail || d.Object.Sender == driverEmail)
                {
                    // Determine if the message is sent by the current user
                    d.Object.IsSentByCurrentUser = d.Object.Sender == driverEmail;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ChatMessages.Add(d.Object);
                        RecentChatsView.ScrollTo(ChatMessages.Last(), animate: true);
                    });
                }
            });
        }


        private async Task<string> GetUserNameByEmail(string email)
        {
            var drivers = await client.Child("Driver").OnceAsync<Driver>();
            var driver = drivers.FirstOrDefault(d => d.Object.Email == email);
            if (driver != null) return driver.Object.Name;

            var parents = await client.Child("Parents").OnceAsync<Parent>();
            var parent = parents.FirstOrDefault(p => p.Object.Email == email);
            return parent?.Object.Name ?? email;
        }

        private async void OnSendMessageClicked(object sender, EventArgs e)
        {
            var message = new ChatMessage
            {
                Sender = driverEmail,
                Receiver = parentEmail,
                MessageText = MessageEntry.Text,
                IsSentByCurrentUser = true,
                Timestamp = DateTime.Now
            };

            await client.Child("chats").PostAsync(message);
            MessageEntry.Text = string.Empty;
        }

        private void OnExitTapped(object sender, EventArgs e)
        {
            if (sender is Label label)
            {
                var activity = Platform.CurrentActivity;
                activity?.MoveTaskToBack(true);
            }
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
    }

    public class ChatMessage
    {
        public string? Sender { get; set; }
        public string? Receiver { get; set; }
        public string? MessageText { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SenderName { get; set; }
        public bool IsSentByCurrentUser { get; set; }
    }
}
