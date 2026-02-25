using Microsoft.Maui.Controls;
using Firebase.Database;
using System.Collections.ObjectModel;

namespace Kats
{
    public partial class SettingsPage : ContentPage
    {
        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        public ObservableCollection<Parent> ParentList { get; set; } = new ObservableCollection<Parent>();
        public ObservableCollection<Children> ChildrenList { get; set; } = new ObservableCollection<Children>();
        public SettingsPage()
        {
            InitializeComponent();
            
        }
      

    }
}
