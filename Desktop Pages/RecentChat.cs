using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminApp
{
    public class RecentChat
    {
        // Unique ID for each chat
        public string ChatId { get; set; }

        // The email of the parent involved in the chat
        public string ParentEmail { get; set; }

        // The email of the driver involved in the chat
        public string DriverEmail { get; set; }

        // The name of the parent (to display in UI)
        public string ParentName { get; set; }

        // The name of the driver (to display in UI)
        public string DriverName { get; set; }

        // The latest message sent between parent and driver
        public string LatestMessage { get; set; }

        // Timestamp of the latest message
        public DateTime LatestMessageTimestamp { get; set; }

        // Constructor
        public RecentChat(string chatId, string parentEmail, string driverEmail, string parentName, string driverName, string latestMessage, DateTime latestMessageTimestamp)
        {
            ChatId = chatId;
            ParentEmail = parentEmail;
            DriverEmail = driverEmail;
            ParentName = parentName;
            DriverName = driverName;
            LatestMessage = latestMessage;
            LatestMessageTimestamp = latestMessageTimestamp;
        }

        // Default constructor
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public RecentChat() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}
