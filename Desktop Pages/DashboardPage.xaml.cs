using Firebase.Database;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.Popup;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Syncfusion.Maui.Buttons;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using Syncfusion.Maui.Charts;
using System.Threading.Tasks;
using Firebase.Database.Query;
using System;
using Syncfusion.Maui.SunburstChart;
using CheckedChangedEventArgs = Syncfusion.Maui.Buttons.CheckedChangedEventArgs;
using static AdminApp.DashboardPage;

namespace AdminApp;

public partial class DashboardPage : ContentPage
{
    FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
    public ObservableCollection<CheckInGridDisplay> CheckInHistoryList = new ObservableCollection<CheckInGridDisplay>();

    public class CheckInGridDisplay : CheckInHistory
    {
        public string? Name { get; set; }
        public string? BusNumber { get; set; }
        public string? Grade { get; set; }
    }

    public DashboardPage()
    {
        InitializeComponent();

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //PopulateCheckIns();
        LoadCheckIns();
        PopulateFrames();
    }

    public async void LoadCheckIns()
    {
        var checkIns = await client.Child("CheckIns").OnceAsync<CheckInHistory>();
        foreach (var _checkIn in checkIns)
        {
            CheckInHistoryList.Add(new CheckInGridDisplay
            {
                Child = _checkIn.Object.Child,
                Name = _checkIn.Object.Child.Name,
                BusNumber = _checkIn.Object.Child.BusNumber,
                Grade = _checkIn.Object.Child.Grade,
                CheckedIn = _checkIn.Object.CheckedIn,
                TimeStamp = _checkIn.Object.TimeStamp,
            });
            //await DisplayAlert(_checkIn.Object.Child.Name, _checkIn.Object.CheckedIn.ToString() + "\n" + _checkIn.Object.TimeStamp, "OK");
        }
        checkInDataGrid.ItemsSource = CheckInHistoryList;
    }

    public async void PopulateCheckIns()
    {
        int count = 0;
        var parents = await client.Child("Parent").OnceAsync<Parent>();
        foreach (var parent in parents)
        {
            foreach (var child in parent.Object.Children)
            {
                if (count == 0)
                {
                    var checkInChild = new CheckInHistory
                    {
                        Child = child,
                        CheckedIn = true,
                        TimeStamp = "01/10/2024 09:00:00",
                    };
                    count++;
                    await client.Child("CheckIns").PostAsync(checkInChild);
                }
                else
                {
                    var checkInChild = new CheckInHistory
                    {
                        Child = child,
                        CheckedIn = false,
                        TimeStamp = "01/10/2024 09:00:00",
                    };
                    count--;
                    await client.Child("CheckIns").PostAsync(checkInChild);
                }                
            }
        }
        await DisplayAlert("Success!", "Tables populated...", "OK");
    }
    public async void PopulateFrames()
    {
        var parents = await client.Child("Parent").OnceAsync<Parent>();
        LabelTotalParents.Text = parents.Count.ToString();
        var drivers = await client.Child("Driver").OnceAsync<Driver>();
        LabelTotalDrivers.Text = drivers.Count.ToString();
        var routes = await client.Child("Routes").OnceAsync<Routes>();
        LabelTotalRoutes.Text = routes.Count.ToString();
        var _childrenList = new List<Children>();
        foreach (var parent in parents)
        {
            foreach (var child in parent.Object.Children)
            {
                _childrenList.Add(child);
            }
        }
        LabelTotalChildren.Text = _childrenList.Count.ToString();
    }

    private async void SfRadialMenuItem_BackItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
    private async void SfRadialMenuItem_NotificationItemTapped(object sender, Syncfusion.Maui.RadialMenu.ItemTappedEventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(NotificationsPage)}");
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
        await DisplayAlert("Error:", "You are already on the selected page...", "OK");
    }
}