using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using Firebase.Database;
using AdminApp;


namespace AdminApp
{
    public class FirebaseService
    {
        //private FirebaseClient client = new FirebaseClient("https://x-bus-75dd1-default-rtdb.firebaseio.com/");

        //public async Task<Routes?> GetRouteByAreaAsync(string area)
        //{
            //var routes = await client.Child("Routes").OnceAsync<Routes>();

            // Ensure routes is not null and handle it safely
            //var routeList = routes.Select(r => r.Object).Where(r => r != null).ToList();

            // Check if routeList is not empty
            //return routeList.FirstOrDefault(r => r?.Stop != null && r.Stop.Any(s => s.Area == area));
        //}

    }

}
