using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class AuthenticationService
{
    private const string FirebaseApiKey = "AIzaSyBjgmy8IhkWUOk6JtJkthEy-OXEfbSok68";
    private const string FirebaseAuthUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
    private const string UserInfoUrl = "https://identitytoolkit.googleapis.com/v1/accounts:lookup?key=";

    public string? IdToken { get; private set; }

    public async Task<string> LoginAsync(string email, string password)
    {
        using var httpClient = new HttpClient();

        var requestBody = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(FirebaseAuthUrl + FirebaseApiKey, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);
            IdToken = authResponse?.IdToken; // Store the ID Token
            return authResponse?.LocalId ?? String.Empty; // Return the user ID
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            throw new Exception("Login failed: " + errorResponse);
        }
    }

    public async Task<string> GetUserEmailAsync()
    {
        if (string.IsNullOrEmpty(IdToken))
        {
            throw new InvalidOperationException("User is not logged in.");
        }

        using var httpClient = new HttpClient();

        var requestBody = new
        {
            idToken = IdToken
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(UserInfoUrl + FirebaseApiKey, content);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var userInfoResponse = JsonConvert.DeserializeObject<UserInfoResponse>(jsonResponse);
            return userInfoResponse?.Users?[0]?.Email ?? String.Empty; // Return the email of the first user
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            throw new Exception("Failed to retrieve user info: " + errorResponse);
        }
    }
}

public class AuthResponse
{
    public string? LocalId { get; set; }
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
}

public class UserInfoResponse
{
    public User[]? Users { get; set; }

    public class User
    {
        public string? Email { get; set; }
    }
}
