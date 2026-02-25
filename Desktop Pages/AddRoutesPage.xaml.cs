using Google.Apis.Http;
using GoogleApi.Entities.Maps.Directions.Response;
using GoogleApi.Entities.Search.Common;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Reactive.Threading.Tasks;
using System.Xml.Linq;
using System.Timers;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using System.Reflection.Metadata.Ecma335;

namespace AdminApp;

public partial class AddRoutesPage : ContentPage
{
    public class AddressComponent
    {
        public string? long_name { get; set; }
        public string? short_name { get; set; }
        public List<string>? types { get; set; }
    }

    new public class Bounds
    {
        public Location? northeast { get; set; }
        public Location? southwest { get; set; }
    }

    public class Location
    {
        public double? lat { get; set; }
        public double? lng { get; set; }
    }

    public class Geometry
    {
        public Bounds? bounds { get; set; }
        public Location? location { get; set; }
        public string? location_type { get; set; }
        public Bounds? viewport { get; set; }
    }

    public class Result
    {
        public List<AddressComponent>? address_components { get; set; }
        public string? formatted_address { get; set; }
        public Geometry? geometry { get; set; }
        public bool? partial_match { get; set; }
        public List<string>? types { get; set; }
    }

    public class RootObject
    {
        public List<Result>? results { get; set; }
        public string? status { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Distance
    {
        public string? text { get; set; }
        public int? value { get; set; }
    }

    public class Duration
    {
        public string? text { get; set; }
        public int? value { get; set; }
    }

    public class EndLocation
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class GeocodedWaypoint
    {
        public string? geocoder_status { get; set; }
        public string? place_id { get; set; }
        public List<string>? types { get; set; }
    }

    public class Leg
    {
        public Distance? distance { get; set; }
        public Duration? duration { get; set; }
        public string? end_address { get; set; }
        public EndLocation? end_location { get; set; }
        public string? start_address { get; set; }
        public StartLocation? start_location { get; set; }
        public List<Step>? steps { get; set; }
        public List<object>? traffic_speed_entry { get; set; }
        public List<object>? via_waypoint { get; set; }
    }

    public class Northeast
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class OverviewPolyline
    {
        public string? points { get; set; }
    }

    public class Polyline
    {
        public string? points { get; set; }
    }

    public class Root
    {
        public List<GeocodedWaypoint>? geocoded_waypoints { get; set; }
        public List<Route>? routes { get; set; }
        public string? status { get; set; }
    }

    public class Route
    {
        public Bounds? bounds { get; set; }
        public string? copyrights { get; set; }
        public List<Leg>? legs { get; set; }
        public OverviewPolyline? overview_polyline { get; set; }
        public string? summary { get; set; }
        public List<object>? warnings { get; set; }
        public List<object>? waypoint_order { get; set; }
    }

    public class Southwest
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class StartLocation
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Step
    {
        public Distance? distance { get; set; }
        public Duration? duration { get; set; }
        public EndLocation? end_location { get; set; }
        public string? html_instructions { get; set; }
        public Polyline? polyline { get; set; }
        public StartLocation? start_location { get; set; }
        public string? travel_mode { get; set; }
        public string? maneuver { get; set; }
    }    
    public class RouteDisplay : Routes
    {
        public string? Key { get; set; }
        public string? areas { get; set; }
        public string? routeimage { get; set; }
    }
    public class uriStop : Stop
    {
        public string? colour { get; set; }
        public string? size { get; set; }
    }
    public class cvStop : uriStop
    {
        public string? stopnumber { get; set; }
        public string? coordinates { get; set; }
        public string? stopimage { get; set; }
    }
    FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
    public List<uriStop> StopsList { get; set; } = new List<uriStop>();
    public List<uriStop> _editStopsList { get; set; } = new List<uriStop>();
    public ObservableCollection<cvStop> cvStopsList { get; set; } = new ObservableCollection<cvStop>();
    public ObservableCollection<cvStop> _editCVStopsList { get; set; } = new ObservableCollection<cvStop>();
    public ObservableCollection<RouteDisplay> routeList { get; set; } = new ObservableCollection<RouteDisplay>();
    public List<uriStop> tempList { get; set; } = new List<uriStop>();
    public List<string> colourList { get; set; } = new List<string>
    {
        "red", "green", "blue", "black", "white", "gray", "brown", "yellow", "purple", "orange",
        "red", "green", "blue", "black", "white", "gray", "brown", "yellow", "purple", "orange",
        "red", "green", "blue", "black", "white", "gray", "yellow", "purple", "orange", "red", "green", "blue"
    };
    public List<string> sizeList { get; set; } = new List<string>
    {
        "normal", "mid", "small", "tiny"
    };
    private System.Timers.Timer? _tmrDelaySearch;
    private const int DelayedTextChangedTimeout = 1500;
    private int userInput = 0;
    private string mapZoom = "12";
    private string centerIndex = String.Empty;
    private bool? _editMode = false;
    private string? _editKey = String.Empty;
    private string? _editStopKey = String.Empty;

    public List<uriStop> returnTestStops()
    {
        return new List<uriStop>
        {
            new uriStop{Area = "Cresta, Randburg, Johannesburg", Coordinates = new Coordinates{ Latitude = -26.1299852, Longitude = 27.9779296}, colour = "blue"},
            new uriStop{Area = "15A Cradock Ave, Rosebank, Johannesburg, 2196", Coordinates = new Coordinates{ Latitude = -26.14636, Longitude = 28.04188}, colour = "orange"},
            new uriStop{Area = "Houghton Estate, Johannesburg, 2198", Coordinates = new Coordinates{ Latitude = -26.1648722, Longitude = 28.0625177}, colour = "yellow"},
            new uriStop{Area = "23 Annet Rd, Cottesloe, Johannesburg, 2092", Coordinates = new Coordinates{ Latitude = -26.1884481, Longitude = 28.0164359}, colour = "red"},
        };
    }
    public async Task<List<RouteDisplay>> GetRoutes()
    {
        return (await client
              .Child("Routes")
              .OnceAsync<Routes>()).Select(item => new RouteDisplay
              {
                  Key = item.Key,
                  Stops = item.Object.Stops,
              }).ToList();
    }

    public void PopulateStops(RouteDisplay rdisplay, List<uriStop> _editStopsList, ObservableCollection<cvStop> _editCVStopsList)
    {
        for (int i = 0; i < rdisplay?.AreaNames.Count; i++)
        {
            _editStopsList.Add(new uriStop
            {
                Area = rdisplay.AreaNames[i],
                Coordinates = rdisplay.Stops["Stop" + (rdisplay.AreaNames.IndexOf(rdisplay.AreaNames[i]) + 1)].Coordinates,
                colour = colourList[i],
                size = "normal"
            });
            _editCVStopsList.Add(new cvStop
            {
                Area = rdisplay.AreaNames[i],
                Coordinates = rdisplay.Stops["Stop" + (rdisplay.AreaNames.IndexOf(rdisplay.AreaNames[i]) + 1)].Coordinates,
                colour = colourList[i],
                size = "normal",
                coordinates = rdisplay?.Stops["Stop" + (rdisplay.AreaNames.IndexOf(rdisplay.AreaNames[i]) + 1)]?.Coordinates?.Latitude + ", " + rdisplay?.Stops["Stop" + (rdisplay.AreaNames.IndexOf(rdisplay.AreaNames[i]) + 1)]?.Coordinates?.Longitude,
                stopimage = colourList[i] + "pin.png",
                stopnumber = "Stop " + (_editCVStopsList.Count + 1).ToString() + ": "
            });

        }
    }
    public void PopulateLists(List<RouteDisplay> rlist, List<uriStop> uriList)
    {
        string temp = String.Empty;
        foreach (var r in rlist)
        {
            foreach (var s in r.AreaNames)
            {
                temp += s + ";\n";
            }
            routeList.Add(new RouteDisplay
            {
                Key = r.Key,
                Stops = r.Stops,
                areas = temp,
                routeimage = "redroute.png",
            }); ;
            temp = String.Empty;
        }

        foreach (uriStop stop in uriList) // Populate collection view list temporarily with preview test items
        {
            if (stop.Coordinates != null)
            {
                cvStopsList.Add(new cvStop
                {
                    Area = stop.Area,
                    Coordinates = stop.Coordinates,
                    coordinates = stop.Coordinates.Latitude.ToString().Replace(',', '.') + ", " + stop.Coordinates.Longitude.ToString().Replace(',', '.'),
                    colour = stop.colour,
                    size = stop.size,
                    stopnumber = "Stop " + (cvStopsList.Count + 1).ToString() + ": ",
                    stopimage = stop.colour + "pin.png",
                });
            }
        }
        collectionViewAllRoutes.ItemsSource = routeList;
        collectionViewPreview.ItemsSource = cvStopsList;
    }    

    public async void InitializeInput()
    {
        while (true)
        {
            userInput = 4;
            try
            {
                userInput = Int32.Parse(await DisplayPromptAsync("How many stops would you like to add?", "The default value is 4. The max value is 16.", "OK", "Cancel", "4", 2, Keyboard.Numeric, "4"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await DisplayAlert("Error!", "Incorrect input. Please try again.", "OK");
                continue;
            }
            if (userInput > 32 || userInput < 2)
            {
                await DisplayAlert("Error: ", "The max value is 32 and the min value is 2, please enter another value.", "OK");
                continue;
            }

            staticMapImage.Source = ImageSource.FromUri(new Uri(await GenerateMapURI(tempList)));
            labelCurrentStop.Text = "0/";
            labelTotalStops.Text = "/" + userInput.ToString();
            stopsProgressBar.SegmentCount = userInput;
            stopsProgressBar.Progress = 0;
            break;
        }
    }

    public async Task<string> GenerateMapURI(List<uriStop> stops, string zoom = "12", string center = "", string resolution = "1280x480", string key = "AIzaSyD4iT2YyOkzbobwrHaLeCNiWbG8TyLyzic")
    {
        if (stops != null && stops.Count > 0)
        {
            string URI = "https://maps.googleapis.com/maps/api/staticmap?";
            URI += "size=" + resolution + "&scale=2";
            URI += "&maptype=roadmap";
            for (int i = 0; i < stops.Count; i++)
            {
                if (stops[i] != null )
                {
                    URI += "&markers=size:" + stops[i].size + "%7Ccolor:" + stops[i].colour + "%7Clabel:" + (i + 1).ToString() + "%7C" + stops[i]?.Coordinates?.Latitude.ToString() + "," + stops[i]?.Coordinates?.Longitude.ToString();
                    //Debug.WriteLine(stops[i].Coordinates.Latitude.ToString() + "," + stops[i].Coordinates.Longitude.ToString());
                    if (i < stops.Count - 1 && stops.Count > 1)
                    {
                        string? origin = stops[i].Area ?? String.Empty;
                        string? destination = stops[i + 1].Area ?? String.Empty;
                        var directions = await ReturnDirections(origin, destination);
                        if (directions.status == "OK")
                        {
                            URI += "&path=enc:" + ReturnPath(directions);
                        }
                        else
                        {
                            await DisplayAlert("Error: ", "No directions found... refresh the page and consider another location.", "OK");
                        }
                    }
                }
                else
                {
                    return String.Empty;
                }
            }
            if (String.IsNullOrWhiteSpace(center) == true)
            {
                if (stops.Count > 1)
                {
                    URI += "&center=" + stops[Convert.ToInt32(stops.Count / 2)]?.Area?.ToString().Replace(' ', '+');
                    //Debug.WriteLine(Convert.ToInt32(stops.Count / 2).ToString());
                }
                else
                {
                    URI += "&center=" + stops[0]?.Area?.ToString().Replace(' ', '+');
                }
            }
            else
            {
                URI += "&center=" + stops[Int32.Parse(center) - 1]?.Area?.ToString().Replace(' ', '+');
            }
            URI += "&zoom=" + zoom;
            URI += "&key=" + key;
            //Debug.WriteLine(URI);
            return URI;
        }
        return String.Empty;
    }

    public async Task<RootObject> ReturnLocation(string address)
    {
        string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/json?key={1}&address={0}&sensor=false", Uri.EscapeDataString(address), "AIzaSyD4iT2YyOkzbobwrHaLeCNiWbG8TyLyzic");
        var results = new RootObject();

        using (var client = new HttpClient())
        {
            var request = await client.GetAsync(requestUri);
            var content = await request.Content.ReadAsStringAsync();
            //Debug.WriteLine(content);
            results = JsonConvert.DeserializeObject<RootObject>(content);
        }
        if (results == null)
            throw new ArgumentNullException();
        return results;
    }

    public async Task<Root> ReturnDirections(string origin, string destination)
    {
        string requestUri = string.Format("https://maps.googleapis.com/maps/api/directions/json?origin={0}&destination={1}&mode=driving&key={2}", Uri.EscapeDataString(origin), Uri.EscapeDataString(destination), "AIzaSyD4iT2YyOkzbobwrHaLeCNiWbG8TyLyzic");
        var results = new Root();

        using (var client = new HttpClient())
        {
            var request = await client.GetAsync(requestUri);
            var content = await request.Content.ReadAsStringAsync();
            //Debug.WriteLine(content);
            results = JsonConvert.DeserializeObject<Root>(content);
        }
        if (results == null || String.IsNullOrWhiteSpace(origin) == true || String.IsNullOrWhiteSpace(destination) == true)
            throw new ArgumentNullException();
        return results;
    }

    public string? ReturnStreetAddress(RootObject root)
    {
        if (root.results == null)
            throw new ArgumentNullException();
        return root?.results[0]?.formatted_address?.ToString();
    }

    public Coordinates ReturnLocationCoordinates(RootObject root)
    {
        return new Coordinates { Latitude = Convert.ToDouble(root?.results?[0]?.geometry?.location?.lat), Longitude = Convert.ToDouble(root?.results?[0]?.geometry?.location?.lng) };
    }

    public string? ReturnPath(Root root)
    {
        if (root.routes == null)
            throw new ArgumentNullException();
        return root?.routes[0]?.overview_polyline?.points?.ToString();
    }
    
    public AddRoutesPage()
    {
        InitializeComponent();
        //staticMapImage.Source = ImageSource.FromUri(new Uri(staticMapURL));
        BindingContext = this;

    }
    protected async override void OnAppearing()
    {       
        base.OnAppearing();
        var rList = await GetRoutes();
        tempList = returnTestStops();       
        PopulateLists(rList, tempList);                
        expanderPreview.IsExpanded = true;
        await Task.Delay(1000);
        InitializeInput();
    }
        
    private void SearchEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_tmrDelaySearch != null)
        {
            SearchLayout.Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#424242");
            _tmrDelaySearch.Stop();
        }

        if (_tmrDelaySearch == null)
        {
            _tmrDelaySearch = new System.Timers.Timer();
            _tmrDelaySearch.Elapsed += this.Tick;
            _tmrDelaySearch.Interval = DelayedTextChangedTimeout;
        }
        SearchLayout.Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#616161");
        _tmrDelaySearch.Start();

    }

    private async void Tick(object? sender, ElapsedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            SearchEntry.IsEnabled = false;
            busyIndicator.IsRunning = true;
            busyIndicator.Title = "Loading...";

        });
        Debug.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
        _tmrDelaySearch?.Stop();
        if (String.IsNullOrWhiteSpace(SearchEntry.Text) == false && (SearchEntry.Text).Length > 2)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var LocationObject = await ReturnLocation(SearchEntry.Text); // initialize location object according to user search
                if (LocationObject != null && LocationObject.results != null && LocationObject.status == "OK") // check if matches have been found for location object
                {                   
                    bool check = await DisplayAlert("Confirm Address:", "Results: " + LocationObject.results[0].formatted_address, "Yes", "No"); // retrieve formatted address of location object and ask user to confirm
                    
                    if (check) // code will only run if user confirms address
                    {
                        if (_editMode == false)
                        {
                            var selectedColour = await DisplayActionSheet("Choose marker colour: ", "Cancel", null, colourList.ToArray()); // display colour options
                            var selectedSize = await DisplayActionSheet("Choose marker size: ", "Cancel", null, sizeList.ToArray()); // display size options

                            StopsList.Add(new uriStop //List of stops to be added to the firebase
                            {
                                Area = LocationObject.results[0].formatted_address,
                                Coordinates = new Coordinates
                                {
                                    Latitude = ReturnLocationCoordinates(LocationObject).Latitude,
                                    Longitude = ReturnLocationCoordinates(LocationObject).Longitude
                                },
                                colour = selectedColour,
                                size = selectedSize
                            });
                            string uriLocation = String.Empty; //Reinitialize URI string for map display

                            if (StopsList.Count == 1) //On the first stop, prompt the user to select desired zoom or automate, as well as clear collection view list of temporary previews, 
                            {
                                mapZoom = await DisplayPromptAsync("Select Zoom Level: ", "The default value is 12... or automate.", "OK", "Auto", "12", 2, Keyboard.Numeric, "12");
                                cvStopsList.Clear(); //Clear the collection view list of temporary values
                            }
                            cvStopsList.Add(new cvStop //Add new stop item with stop number for collection view
                            {
                                Area = LocationObject.results[0].formatted_address,
                                Coordinates = new Coordinates
                                {
                                    Latitude = ReturnLocationCoordinates(LocationObject).Latitude,
                                    Longitude = ReturnLocationCoordinates(LocationObject).Longitude
                                },
                                colour = selectedColour,
                                size = selectedSize,
                                coordinates = ReturnLocationCoordinates(LocationObject).Latitude.ToString().Replace(',', '.') + ", " + ReturnLocationCoordinates(LocationObject).Longitude.ToString().Replace(',', '.'),
                                stopnumber = "Stop " + (cvStopsList.Count + 1).ToString() + ": ",
                                stopimage = selectedColour + "pin.png",
                            });
                            busyIndicator.Title = "Searching...";
                            uriLocation = await GenerateMapURI(StopsList, mapZoom); //Generate Map URI for display
                            staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation)); //Display newly generated map
                            expanderPreview.IsExpanded = false; //Refresh collection views for stops preview
                            expanderPreview.IsExpanded = true; //Refresh collection views for stops preview
                            await DisplayAlert("Success!", "Map preview updated.", "Ok");
                            double addition = 100 / userInput;
                            stopsProgressBar.Progress = addition * StopsList.Count; //Update progress bar

                            if (StopsList.Count == userInput) //All stops have been added according to initial user specifications
                            {
                                // Visual changes once all stops have been added
                                stopsProgressBar.Progress = 100;
                                labelCurrentStop.Text = StopsList.Count.ToString() + "/";
                                SearchLayout.Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#388E3C");
                                SearchLayout.UnfocusedStrokeThickness = 1.5;
                                SearchEntry.Text = String.Empty;
                                SearchEntry.IsEnabled = false;
                                SearchEntry.Placeholder = "Success! All stops added.";

                                await DisplayAlert("All Stops Added.", "Finalize zoom level and centered stop for Route preview.", "OK"); // Display alert indicating all stops have been added
                                bool checkConfirm = false;
                                while (!checkConfirm)
                                {
                                    mapZoom = await DisplayPromptAsync("Select Zoom Level: ", "The default value is 12... or automate.", "OK", "Auto", "12", 2, Keyboard.Numeric, "12");
                                    if (String.IsNullOrWhiteSpace(mapZoom) == false)
                                    {
                                        if (Int32.Parse(mapZoom) > 20 || Int32.Parse(mapZoom) < 0)
                                        {
                                            await DisplayAlert("Error!", "Please enter a value greater than or equal to 1, or less than or equal to 20.", "OK");
                                            continue;
                                        }
                                    }
                                    int tempIndex = StopsList.Count / 2;
                                    centerIndex = await DisplayPromptAsync("Select Center Focused Stop Number: ", $"The default value is {tempIndex}...", "OK", "Cancel", tempIndex.ToString(), 2, Keyboard.Numeric, tempIndex.ToString());
                                    if (String.IsNullOrWhiteSpace(centerIndex) == false)
                                    {
                                        if (Int32.Parse(centerIndex) > StopsList.Count || Int32.Parse(centerIndex) < 1)
                                        {
                                            await DisplayAlert("Error!", "You cannot center a stop that is not in the route!", "OK");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        await DisplayAlert("Error!", "A value must be selected!", "OK");
                                        continue;
                                    }
                                    uriLocation = await GenerateMapURI(StopsList, mapZoom, centerIndex);
                                    staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));
                                    checkConfirm = await DisplayAlert("Confirm Zoom and Center?", "Or continue finalization of the map...", "Confirm", "Repeat");
                                    if (checkConfirm == false)
                                    {
                                        busyIndicator.IsRunning = true;
                                        busyIndicator.Title = "Loading...";
                                        await Task.Delay(5000);
                                        busyIndicator.IsRunning = false;
                                        busyIndicator.Title = "Searching...";
                                    }
                                }
                                // FINALIZED ROUTE CODE HERE                                                        
                            }
                        }
                        else if (_editMode == true)
                        {
                            if (String.IsNullOrWhiteSpace(_editKey) == false && String.IsNullOrWhiteSpace(_editStopKey) == false)
                            {
                                //EDIT CODE HERE???
                                //rdisplay.Stops["Stop" + (rdisplay.AreaNames.IndexOf(rdisplay.AreaNames[i]) + 1)
                                while (true)
                                {
                                    uriStop? _oldStop = _editStopsList.FirstOrDefault(p => p.Area == _editStopKey); //Old stop object and associated index
                                    if (_oldStop == null)
                                        throw new ArgumentNullException();
                                    int _oldStopIndex = _editStopsList.IndexOf(_oldStop);

                                    uriStop _newURIStop = new uriStop // object for new URI stop object
                                    {
                                        Area = LocationObject.results[0].formatted_address,
                                        Coordinates = new Coordinates
                                        {
                                            Latitude = ReturnLocationCoordinates(LocationObject).Latitude,
                                            Longitude = ReturnLocationCoordinates(LocationObject).Longitude
                                        },
                                        colour = colourList[_oldStopIndex],
                                        size = "normal"
                                    };

                                    cvStop _newCVStop = new cvStop // object for new collection view stop
                                    {
                                        Area = LocationObject.results[0].formatted_address,
                                        Coordinates = new Coordinates
                                        {
                                            Latitude = ReturnLocationCoordinates(LocationObject).Latitude,
                                            Longitude = ReturnLocationCoordinates(LocationObject).Longitude
                                        },
                                        colour = colourList[_oldStopIndex],
                                        size = "normal",
                                        coordinates = ReturnLocationCoordinates(LocationObject).Latitude.ToString().Replace(',', '.') + ", " + ReturnLocationCoordinates(LocationObject).Longitude.ToString().Replace(',', '.'),
                                        stopimage = colourList[_oldStopIndex] + "pin.png",
                                        stopnumber = "Stop " + (_oldStopIndex + 1).ToString() + ": "
                                    };

                                    _editStopsList[_oldStopIndex] = _newURIStop;
                                    _editCVStopsList[_oldStopIndex] = _newCVStop;
                                    expanderPreview.IsExpanded = false;
                                    expanderPreview.IsExpanded = true;
                                    var uriLocation = await GenerateMapURI(_editStopsList);
                                    staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));
                                    busyIndicator.IsRunning = false;

                                    bool _editCheck = await DisplayAlert("Confirm Route", "Or edit another stop...", "Confirm", "Continue");
                                    if (_editCheck)
                                    {
                                        uriLocation = await GenerateMapURI(_editStopsList);
                                        staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));

                                        try
                                        {
                                            Dictionary<string, Stop> dict = new Dictionary<string, Stop>(); // Initialize Dictionary matching Stops Dictionary structure in Route Class
                                            for (int i = 0; i < _editStopsList.Count; i++)
                                            {
                                                var st = _editStopsList[i];
                                                dict.Add($"Stop{i + 1}", new Stop //Add stops to dictionary
                                                {
                                                    Area = st.Area,
                                                    Coordinates = st.Coordinates,
                                                });
                                            }
                                            var route = new Routes
                                            {
                                                Stops = dict
                                            };
                                            await client.Child("Routes").Child(_editKey).PutAsync(route);
                                            await DisplayAlert("Success!", $"{_editKey} edited successfully.", "OK");
                                            await DisplayAlert("NOTE!", "Remember to edit all users that make use of this route and update the route...", "OK");

                                            // ADD CODE TO EDIT BUS WHERE OLD ROUTE IS PRESENT!!!

                                            var _oldRouteDisplayItem = routeList.Where(p => p.Key == _editKey)?.FirstOrDefault();
                                            if (_oldRouteDisplayItem != null)
                                            {
                                                var _oldRoute = new Routes
                                                {
                                                    Stops = _oldRouteDisplayItem.Stops
                                                };


                                                var buses = await client.Child("Buses").OnceAsync<Bus>();
                                                var bus = buses.FirstOrDefault(p => p.Object.Route == _oldRoute)?.Object;
                                                string? key = buses.FirstOrDefault(p => p.Object.Route == _oldRoute)?.Key;
                                                if (bus != null)
                                                {
                                                    bus.Route = route;
                                                    await client.Child("Buses").Child(key).PatchAsync(bus);

                                                    var rlist = await GetRoutes();
                                                    routeList.Clear();

                                                    string temp = String.Empty;
                                                    foreach (var r in rlist)
                                                    {
                                                        foreach (var s in r.AreaNames)
                                                        {
                                                            temp += s + ";\n";
                                                        }
                                                        routeList.Add(new RouteDisplay
                                                        {
                                                            Key = r.Key,
                                                            Stops = r.Stops,
                                                            areas = temp,
                                                            routeimage = "redroute.png",
                                                        }); ;
                                                        temp = String.Empty;
                                                    }
                                                    uriLocation = await GenerateMapURI(StopsList);
                                                    staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));
                                                }
                                                else
                                                {
                                                    await DisplayAlert("Error!", "Bus error! Contact administrators...", "OK");
                                                }
                                            }
                                            else
                                            {
                                                await DisplayAlert("Error!", "Route not found", "OK");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine(ex.Message);
                                            await DisplayAlert("Error!", "Unexpected error occurred. Try again or refresh the page...", "OK");
                                        }
                                        _editMode = false;
                                        collectionViewPreview.ItemsSource = cvStopsList;
                                        expanderPreview.IsExpanded = false;
                                        expanderPreview.IsExpanded = true;
                                        break;
                                    }
                                    else
                                    {
                                        while (true)
                                        {
                                            var areaNames = _editStopsList.Select(stop => stop.Area ?? "Unknown Area").ToArray();
                                            _editStopKey = await DisplayActionSheet("Select stop to edit: ", "Cancel", null, areaNames);
                                            if (_editStopKey == "Cancel" || String.IsNullOrWhiteSpace(_editStopKey) == true)
                                            {
                                                await DisplayAlert("Error!", "A stop must be selected...", "OK");
                                                continue;
                                            }
                                            break;
                                        }
                                        break;
                                    }
                                }                                
                            }
                            else
                            {
                                await DisplayAlert("Unexpected Error:", "Failed to load selected data...", "OK");
                            }
                        }
                        // Visual changes once adding stop process has begun
                        SearchEntry.Text = String.Empty;
                        SearchLayout.Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#0e79d7");                        
                    }
                }
                else
                {
                    await DisplayAlert("Error:", "No matches found...", "Ok");
                }
                //EDIT CODE HERE???
            });
        }
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (StopsList.Count != userInput) // code that will always run so long as all stops have not been added.
            {
                SearchEntry.IsEnabled = true;
                labelCurrentStop.Text = StopsList.Count.ToString() + "/";
                SearchLayout.Stroke = Microsoft.Maui.Graphics.Color.FromArgb("#424242");
            }
            busyIndicator.IsRunning = false;
        });
    }

    
    private async void collectionViewAllRoutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RouteDisplay selectedItem = (RouteDisplay)collectionViewAllRoutes.SelectedItem;
        string _selectedOption = String.Empty;
        _editKey = String.Empty;
        _editStopKey = String.Empty;
        _editMode = false;

        while (true)
        {
            var selectedOption = await DisplayActionSheet("Choose one of the following...", "Cancel", null, ["Edit", "Delete"]);
            if (String.IsNullOrWhiteSpace(selectedOption) == false)
            {
                _selectedOption = selectedOption;
                break;
            }
            continue;
        }
        
        if (selectedItem != null)
        {
            if (_selectedOption == "Edit" && selectedItem.Key != null)
            {
                _editKey = selectedItem.Key;
                _editMode = true;

                var routeStops = await client
                    .Child($"Routes/{_editKey}/Stops")
                    .OnceAsync<Stop>();
                var areaNames = routeStops.Select(stop => stop.Object?.Area ?? "Unknown Area").ToArray();

                await DisplayAlert("Edit Mode Enabled:", "Click the 'Cancel' button at the top right to undo this change...", "OK");
                _editStopsList.Clear();
                _editCVStopsList.Clear();
                _editStopKey = await DisplayActionSheet("Select stop to edit: ", "Cancel", null, areaNames);
                PopulateStops(selectedItem, _editStopsList, _editCVStopsList);
                collectionViewPreview.ItemsSource = _editCVStopsList;

                var uriLocation = await GenerateMapURI(_editStopsList);
                staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));
                expanderPreview.IsExpanded = false;
                expanderPreview.IsExpanded = true;
                return;
            }
            else if (_selectedOption == "Delete")
            {
                bool check = await DisplayAlert("Confirm", "Are you sure you want to Delete this route? All changes are permanent...", "Yes", "No");
                if (check)
                {
                    //int st = routeList.IndexOf(selectedItem);
                    routeList.Remove(selectedItem);
                    for (int i = 0; i < routeList.Count; i++)
                    {
                        Routes route = new Routes
                        {
                            Stops = routeList[i].Stops
                        };
                        await client.Child("Routes").Child($"Route{i + 1}").PutAsync(route);
                    }
                    await client.Child("Routes/" + $"Route{routeList.Count + 1}").DeleteAsync();
                    await DisplayAlert("Success!", $"{selectedItem.Key} deleted successfully.", "OK");
                }                            
            }
            return;
        }
        return;
    }
    private async void CancelRoute_Clicked(object sender, EventArgs e)
    {
        if (_editMode == false)
        {
            //REFRESHING
            // Get current page
            var page = Navigation.NavigationStack.LastOrDefault();

            // Load new page
            await Shell.Current.GoToAsync(nameof(AddRoutesPage), false);

            // Remove old page
            Navigation.RemovePage(page);
        }
        else
        {
            _editMode = false;
            if (StopsList.Count > 0)
            {
                var uriLocation = await GenerateMapURI(StopsList);
                staticMapImage.Source = ImageSource.FromUri(new Uri(uriLocation));
            }            
            collectionViewPreview.ItemsSource = cvStopsList;
            expanderPreview.IsExpanded = false;
            expanderPreview.IsExpanded = true;
            await DisplayAlert("Edit Mode Disabled:", "Your create route progress has been preserved.", "OK");
        }

    }
    private async void AddRoute_Clicked(object sender, EventArgs e)
    {
        AddRoute.IsEnabled = false;

        if (StopsList.Count > 0 && (StopsList.Count == userInput))
        {
            
            try
            {
                Dictionary<string, Stop> dict = new Dictionary<string, Stop>(); // Initialize Dictionary matching Stops Dictionary structure in Route Class
                for (int i = 0; i < StopsList.Count; i++)
                {
                    var st = StopsList[i];
                    dict.Add($"Stop{i + 1}", new Stop //Add stops to dictionary
                    {
                        Area = st.Area,
                        Coordinates = st.Coordinates,
                    });
                }
                var route = new Routes
                {
                    Stops = dict
                };
                await client.Child("Routes").Child($"Route{routeList.Count + 1}").PutAsync(route);
                await DisplayAlert("Success!", $"Route{routeList.Count + 1} added successfully.", "OK");
                while (true)
                {
                    string _busnumber = await DisplayPromptAsync("Register Bus: ", "Enter the registration number:", null);
                    if (String.IsNullOrWhiteSpace(_busnumber) == true) continue;
                    
                    var _checkBuses = await client.Child("Buses").OnceAsync<Bus>();
                    var _checkBus = _checkBuses.FirstOrDefault(p => p.Object.BusNumber == _busnumber)?.Object;

                    if (_checkBus != null)
                    {
                        await DisplayAlert("Error!", "This bus number is already in use...", "OK");
                        continue;
                    }
                    else
                    {
                        var _bus = new Bus
                        {
                            Route = route,
                            BusNumber = _busnumber
                        };
                        await client.Child("Buses").PostAsync(_bus);
                        await DisplayAlert("Success!", $"Bus ({_busnumber}) added successfully.", "OK");
                        break;
                    }                    
                }
                //REFRESHING
                // Get current page
                var page = Navigation.NavigationStack.LastOrDefault();

                // Load new page
                await Shell.Current.GoToAsync(nameof(AddRoutesPage), false);

                // Remove old page
                Navigation.RemovePage(page);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await DisplayAlert("Error!", "Unexpected error occurred. Try again or refresh the page...", "OK");
            }
        }
        else
        {
            await DisplayAlert("Error!", "All stops must have been added in order to complete the route...", "OK");
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
        await DisplayAlert("Error:", "You are already on the selected page...", "OK");
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