using Firebase.Database;
using Firebase.Database.Query;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Core.Hosting;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Kats
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("Brands-Regular-400.otf", "FAB");
                    fonts.AddFont("Free-Regular-400.otf", "FAR");
                    fonts.AddFont("Free-Solid-900.otf", "FAS");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Initialize Firebase Admin SDK
            Task.Run(async () => await InitializeFirebaseAdminSdk());

            Task task = RegisterAsync();
            return builder.Build();
        }

        private static async Task InitializeFirebaseAdminSdk()
        {
            try
            {
                // Get the assembly containing the resource
                var assembly = Assembly.GetExecutingAssembly();

                // Construct the resource name
                string resourceName = "Kats.Resources.Raw.xbus.json";

                // Attempt to get the resource stream
                Stream? stream = assembly.GetManifestResourceStream(resourceName);

                // Check if the stream is null
                if (stream == null)
                {
                    throw new FileNotFoundException($"The embedded resource {resourceName} was not found.");
                }

                // Proceed to use the stream safely
                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonContent = await reader.ReadToEndAsync();
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(jsonContent)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing Firebase Admin SDK: {ex.Message}");
            }
        }


        public static async Task RegisterAsync()
        {
            FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");
            var users = await client.Child("Users").OnceAsync<Users>();

            if (users.Count == 0)
            {
                await client.Child("Users").PostAsync(new Users { UserRole = "Parent" });
                await client.Child("Users").PostAsync(new Users { UserRole = "Driver" });
            }
        }
    }
}
