using Firebase.Database;
using Firebase.Database.Query;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Util;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace AdminApp
{

    //[QueryProperty(nameof(Email), "email")]
    public partial class MainPage : ContentPage
    {

        FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
        public ObservableCollection<Parent> ParentList { get; set; } = new ObservableCollection<Parent>();
        public ObservableCollection<ParentChild> FamilyList { get; set; } = new ObservableCollection<ParentChild>();
        public ObservableCollection<Children> ChildrenList { get; set; } = new ObservableCollection<Children>();
        public ObservableCollection<Driver> DriverList { get; set; } = new ObservableCollection<Driver>();
        public ObservableCollection<BusKey> BusList { get; set; } = new ObservableCollection<BusKey>();
        public ObservableCollection<Parent> cvParentList { get; set; } = new ObservableCollection<Parent>();

        public class BusKey : Bus
        {
            public string? Key { get; set; }
        }

        public MainPage()
        {

            InitializeComponent();
            BindingContext = this;
            //collectionView.ItemsSource = GetParents();
            collectionView.ItemsSource = ParentList;
            collectionViewBus.ItemsSource = BusList;
            //collectionView.Background = Microsoft.Maui.Graphics.Color.FromArgb("#757575");

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadUserDetails();
        }
        //POPULATING LISTS OF OBJECTS FOR THE DATABASES
        private async void LoadUserDetails()
        {
            //var drivers = await client.Child("Driver").OnceAsync<Driver>();
            //var dr = drivers.FirstOrDefault(p => p.Object.Name == "Todd Graham");
            //await DisplayAlert("NOTE: ", $"{dr.Key}\n{dr.Object.Email}\n{dr.Object.Name}", "OK");

            //await client.Child("Driver").Child(dr.Key).DeleteAsync();


            var loggedInUserEmail = UserSession.LoggedInUserEmail;
            var parentList = await GetAllParents();

            FamilyList.Clear();
            ParentList.Clear();
            ChildrenList.Clear();
            DriverList.Clear();
            BusList.Clear();

            foreach (var parent in parentList)
            {
                ParentList.Add(parent);
                foreach (var child in parent.Children)
                {
                    ChildrenList.Add(child);
                    FamilyList.Add(new ParentChild
                    {
                        Email = parent.Email,
                        //ParentName = parent.Name,
                        //Role = parent.ParentType,
                        //PhoneNumber = parent.PhoneNumber,
                        Name = child.Name,
                        SchoolName = child.SchoolName,
                        IsFemale = child.IsFemale,
                        IsMale = child.IsMale,
                        Grade = child.Grade,
                        BusNumber = child.BusNumber,
                    });
                }
            }
            var driverList = await GetDrivers();
            foreach (var driver in driverList)
            {
                DriverList.Add(driver);
            }
            var busList = await GetBuses();
            foreach (var bus in busList)
            {
                var routes = await client
                .Child("Routes")
                .OnceAsync<Routes>();
                var Key = routes.Where(p => JsonConvert.SerializeObject(p.Object) == JsonConvert.SerializeObject(bus.Route)).FirstOrDefault()?.Key;
                var busKey = new BusKey
                {
                    Route = bus.Route,
                    BusNumber = bus.BusNumber,
                    Key = Key,
                };
                BusList.Add(busKey);
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
                  Image = item.Object.Image,
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

        // LIST TO RETRIEVE ALL BUSES
        private async Task<List<Bus>> GetBuses()
        {

            return (await client
              .Child("Buses")
              .OnceAsync<Bus>()).Select(item => new Bus
              {
                  BusNumber = item.Object.BusNumber,
                  Route = item.Object.Route,
              }).ToList();
        }
        //DATAGRID ITEMS FOR CREATING LIST OF CHILDREN WITH ASSOCIATED PARENTS
        public class ParentChild : Children
        {
            public string? Email { get; set; }
            //TEST LIST FOR DISPLAYING A LARGE ARRAY OF PLACEHOLDER PARENTS
        }
        private List<ParentTest> GetParents()
        {
            return new List<ParentTest>
                   {
                   new ParentTest {Name="Karen", Role="Mother", Image="emoji3.png"},
                   new ParentTest {Name="Joan", Role="Mother", Image="emoji7.png"},
                   new ParentTest {Name="Stephanie", Role="Mother", Image="emoji3.png"},
                   new ParentTest {Name="Stacy", Role="Mother", Image="emoji7.png"},
                   new ParentTest {Name="John", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Jack", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Joseph", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Jeb", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Karen", Role="Mother", Image="emoji3.png"},
                   new ParentTest {Name="Joan", Role="Mother", Image="emoji7.png"},
                   new ParentTest {Name="Stephanie", Role="Mother", Image="emoji3.png"},
                   new ParentTest {Name="Stacy", Role="Mother", Image="emoji7.png"},
                   new ParentTest {Name="John", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Jack", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Joseph", Role="Father", Image="emoji6.png"},
                   new ParentTest {Name="Jeb", Role="Father", Image="emoji6.png"}
                   };
        }

        //FILTER STUFF
        private string FilterText = String.Empty;
        private void OnFilterChanged()
        {
            if (this.childrenDataGrid != null && this.childrenDataGrid.View != null)
            {
                this.childrenDataGrid.View.Filter = FilterRecords;
                this.childrenDataGrid.View.RefreshFilter();
            }
        }

        private bool FilterRecords(object record)
        {
            var childrenInfo = record as ParentChild;
            if (childrenInfo != null && childrenInfo.Email != null
                && childrenInfo.Name != null && childrenInfo.SchoolName != null
                && childrenInfo.Grade != null && childrenInfo.BusNumber != null)
            {
                if (Conditions.SelectedItem.ToString() == "Record Contains")
                {
                    var filterText = FilterText.ToLower();
                    if (childrenInfo.Email.ToString().ToLower().Contains(filterText) ||
                        childrenInfo.Name.ToString().ToLower().Contains(filterText) ||
                        childrenInfo.SchoolName.ToString().ToLower().Contains(filterText) ||
                        childrenInfo.Grade.ToString().ToLower().Contains(filterText) ||
                        childrenInfo.BusNumber.ToString().ToLower().Contains(filterText))
                        return true;
                    return false;
                }
                else if (Conditions.SelectedItem.ToString() == "Record Equals")
                {
                    if (FilterText.Equals(childrenInfo.Email.ToString()) ||
                        FilterText.Equals(childrenInfo.Name.ToString()) ||
                        FilterText.Equals(childrenInfo.SchoolName.ToString()) ||
                        FilterText.Equals(childrenInfo.Grade.ToString()) ||
                        FilterText.Equals(childrenInfo.BusNumber.ToString()))
                        return true;
                    else return false;

                }
                else
                {
                    if (!FilterText.Equals(childrenInfo.Email.ToString()) &&
                        !FilterText.Equals(childrenInfo.Name.ToString()) &&
                        !FilterText.Equals(childrenInfo.SchoolName.ToString()) &&
                        !FilterText.Equals(childrenInfo.Grade.ToString()) &&
                        !FilterText.Equals(childrenInfo.BusNumber.ToString()))
                        return true;
                    else return false;
                }
            }
            return false;
        }

        private async void filterText_SearchButtonPressed(object sender, EventArgs e)
        {
            await Task.Delay(200);
            this.OnFilterChanged();
        }
        private async void filterText_SearchButtonPressedAlternate(object sender, EventArgs e)
        {
            await Task.Delay(200);
            this.OnFilterChanged();
        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue == null)
            {
                this.FilterText = string.Empty;
            }
            else
            {
                this.FilterText = e.NewTextValue;
            }
        }
        private async void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(200);
            //CollectionView collectionV = sender as CollectionView;
            Parent selectedParent = (Parent)collectionView.SelectedItem;
            if (selectedParent != null)
            {
                if (this.childrenDataGrid != null && this.childrenDataGrid.View != null)
                {
                    this.childrenDataGrid.View.Filter = CollectionViewFilterRecords;
                    this.childrenDataGrid.View.RefreshFilter();
                }
            }
        }

        private bool CollectionViewFilterRecords(object record)
        {
            Parent selectedParent = (Parent)collectionView.SelectedItem;
            var childrenInfo = record as ParentChild;
            if (childrenInfo != null)
            {
                if (selectedParent != null && selectedParent.Email != null && childrenInfo.Email != null)
                {
                    if (selectedParent.Email.ToString().Equals(childrenInfo.Email.ToString()))
                        return true;
                    else return false;
                }
            }
            return false;
        }
        //NAVIGATION BUTTON/S
        private async void OnAddUsersButtonClicked(object sender, EventArgs e)
        {
            //collectionView.SelectedItem = null;
            //collectionView.SelectedItems.Clear();
            if (this.childrenDataGrid.View != null)
            {
                this.childrenDataGrid.View.Filter = null;
                this.childrenDataGrid.View.RefreshFilter();
            }

            await Shell.Current.GoToAsync($"{nameof(AddUsersPage)}");
        }

        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            if (this.childrenDataGrid.View != null)
            {
                this.childrenDataGrid.View.Filter = null;
                this.childrenDataGrid.View.RefreshFilter();
                collectionView.SelectedItem = null;
                collectionView.SelectedItems.Clear();
                //GoToState(true);
                //VisualStateManager.GoToState(collectionView, "Normal");

                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(MainPage), false);

                // Remove old page
                Navigation.RemovePage(page);

            }
            else
            {
                await DisplayAlert("Error", "Refreshing is not supported at this time...", "Ok");
            }
        }

        void GoToState(bool isValid)
        {
            string visualState = isValid ? "Normal" : "Selected";
            VisualStateManager.GoToState(collectionView, visualState);
            //Debug.WriteLine(collectionView.GetChildElements);
        }

        private async void ProfileDetailsButton_Clicked(object sender, EventArgs e)
        {
            Parent selectedParent = (Parent)collectionView.SelectedItem;
            if (selectedParent != null && selectedParent.Email != null)
            {
                string email = selectedParent.Email;
                await Shell.Current.GoToAsync($"{nameof(EditUsersPage)}?email={Uri.EscapeDataString(email)}");
                if (this.childrenDataGrid.View != null)
                {
                    this.childrenDataGrid.View.Filter = null;
                    this.childrenDataGrid.View.RefreshFilter();
                }

            }
            else
            {
                await DisplayAlert("Error", "You have not selected a user. You may do so by clicking on one of the 'Parent' boxes...", "Ok");
            }
        }
        private async void DriverDetailsButton_Clicked(object sender, EventArgs e)
        {
            BusKey selectedBus = (BusKey)collectionViewBus.SelectedItem;
            var drivers = await client.Child("Driver").OnceAsync<Driver>();
            
            if (drivers != null && selectedBus != null)
            {
                var driver = drivers.FirstOrDefault(p => p.Object.bus?.BusNumber == selectedBus.BusNumber)?.Object;
                if (driver != null && driver.Email != null)
                {
                    string email = driver.Email;
                    await Shell.Current.GoToAsync($"{nameof(EditUsersPage)}?email={Uri.EscapeDataString(email)}");
                    if (this.childrenDataGrid.View != null)
                    {
                        this.childrenDataGrid.View.Filter = null;
                        this.childrenDataGrid.View.RefreshFilter();
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "No drivers assigned to this route...", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "You have not selected a bus. You may do so by clicking on one of the 'Bus' boxes...", "Ok");
            }
        }
        private async void RefreshButtonDriver_Clicked(object sender, EventArgs e)
        {
            if (this.driverDataGrid.View != null)
            {
                this.driverDataGrid.View.Filter = null;
                this.driverDataGrid.View.RefreshFilter();

                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(MainPage), false);

                // Remove old page
                Navigation.RemovePage(page);

            }
            else
            {
                await DisplayAlert("Error", "Refreshing is not supported at this time...", "Ok");
            }
        }

        private async void SfRadialMenuItem_BackItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void SfRadialMenuItem_ItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(NotificationsPage)}");
        }

        private async void SfRadialMenuItem_RouteItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(AddRoutesPage)}");
        }
        private async void SfRadialMenuItem_MainItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await DisplayAlert("Error:", "You are already on the selected page...", "OK");
        }
        private async void SfRadialMenuItem_DashboardItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
        {
            await Shell.Current.GoToAsync($"{nameof(DashboardPage)}");
        }

        
    }

}



